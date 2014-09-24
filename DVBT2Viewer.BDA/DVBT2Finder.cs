using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DirectShowLib;
using DVBT2Viewer.BDA.Graph;
using DVBT2Viewer.BDA.Helpers;
using DVBT2Viewer.BDA.Helpers.HelperTypes;
using DVBT2Viewer.BDA.Interfaces;
using DVBT2Viewer.BDA.InternalModels;
using DVBT2Viewer.BDA.Models;

namespace DVBT2Viewer.BDA
{
    internal class DVBT2Finder : DVBT2BaseGraphCommands, IDVBT2Finder
    {
        private readonly DVBT2BaseGraph graph = new DVBT2BaseGraph();

        /// <summary>
        /// Channel info query timeout = 2 seconds
        /// </summary>
        private const int QueryTimeout = 2000;

        /// <summary>
        /// Pause between each channel info query = 100 milliseconds
        /// </summary>
        private const int QueryIterationPause = 100;

        public string TunerDeviceName { get { return graph.TunerDeviceName; } }

        public void Dispose()
        {
            graph.Dispose();
        }

        public void Build()
        {
            graph.BuildGraph();
        }

        public void Run()
        {
            graph.Run();
        }

        public void Stop()
        {
            graph.Stop();
        }

        public LockStatus GetLockStatus()
        {
            return graph.GetLockStatus();
        }

        public void Lock(int frequency, int bandwidth)
        {
            graph.Tune(GetTuneRequest(frequency, bandwidth));
        }

        public DigitalMultiplex GetMultiplex(int frequency, int bandwidth)
        {
            var multiplex = new DigitalMultiplex { Frequency = frequency, Bandwidth = bandwidth };

            // Check if "KSPROPERTY_BDA_PLP_NUMBER" parameter supported
            var plpSupported = KsPropertyHelper.KSSupported(graph.TunerPin, KSPropSets.KSPROPSETID_BdaDigitalDemodulator,
                (int)KSPROPERTY_BDA_DIGITAL_DEMODULATOR.KSPROPERTY_BDA_PLP_NUMBER);

            // Check if SET PLP defined
            var setPlpDefined = plpSupported.HasFlag(KSPropertySupport.Set); 

            // Try to get max PLP number
            int plpMaxCount = 255;
            if (plpSupported.HasFlag(KSPropertySupport.Get))
                plpMaxCount = (int)KsPropertyHelper.KSGetNode(graph.TunerPin, KSPropSets.KSPROPSETID_BdaDigitalDemodulator,
                    (int)KSPROPERTY_BDA_DIGITAL_DEMODULATOR.KSPROPERTY_BDA_PLP_NUMBER, typeof(int));
            // To avoid possible driver bugs
            if (plpMaxCount <= 0)
                plpMaxCount = 255;

            int currentPlp = 0;
            while (currentPlp < plpMaxCount)
            {
                try
                {
                    // Set PLP if applicable
                    if (setPlpDefined)
                        SetPlp(currentPlp);
                    // Get lock 
                    if (!GetLock(frequency, bandwidth))
                        break;
                    var channels = GetChannelList(multiplex);
                    foreach (var channel in channels)
                    {
                        channel.StreamId = setPlpDefined ? currentPlp : -1;
                        multiplex.Channels.Add(channel);
                    }

                    if (!setPlpDefined)
                        break;
                }
                catch (Exception)
                {
                    break;
                }
                currentPlp++;
            }

            return
                multiplex;
        }

        protected override IDVBT2BaseGraphProps GetGraph()
        {
            return graph;
        }

        private bool GetLock(int frequency, int bandwidth)
        {
            graph.Tune(GetTuneRequest(frequency, bandwidth));
            Thread.Sleep(QueryTimeout / 2);

            int timeElapsed = 0;
            while (true)
            {
                var status = graph.GetLockStatus();
                if (status.SignalPresent && status.SignalLocked)
                    return true;
                if (timeElapsed >= QueryTimeout)
                    return false;
                Thread.Sleep(QueryIterationPause);
                timeElapsed += QueryIterationPause;
            }
        }

        #region Get channel list
        private IEnumerable<DigitalChannel> GetChannelList(DigitalMultiplex multiplex)
        {
            var list = new List<DigitalChannel>();
            using (var parser = graph.GetServiceInfoParser())
            {
                // Getting SDT
                var sdtList = GetSDT(parser);
                if (sdtList == null)
                    return null;
                foreach (var item in sdtList)
                {
                    var channel = multiplex.CreateChannel();
                    channel.SID = item.SID;
                    list.Add(channel);
                    DVBTChannelListHelper.ChannelApplySDT(channel, item);
                }

                // Getting PAT & PMT
                var patList = GetPAT(parser);
                if (patList == null)
                    return null;
                foreach (var item in patList)
                {
                    var channel = list.FirstOrDefault(x => x.SID == item.SID);
                    if (channel != null)
                    {
                        DVBTChannelListHelper.ChannelApplyPAT(channel, item);
                        DVBTChannelListHelper.ChannelApplyPMT(channel, GetPMT(parser, item));
                    }
                }
            }
            return list;
        }
        #endregion

        #region Channel table functions
        private static IEnumerable<SDTChannelModel> GetSDT(ServiceInfoParser parser)
        {
            int timeElapsed = 0;
            while (true)
            {
                var result = parser.GetSDT();
                if (result != null)
                    return result;
                if (timeElapsed >= QueryTimeout)
                    return null;
                Thread.Sleep(QueryIterationPause);
                timeElapsed += QueryIterationPause;
            }
        }

        private static IEnumerable<PATChannelModel> GetPAT(ServiceInfoParser parser)
        {
            int timeElapsed = 0;
            while (true)
            {
                var result = parser.GetPAT();
                if (result != null)
                    return result;
                if (timeElapsed >= QueryTimeout)
                    return null;
                Thread.Sleep(QueryIterationPause);
                timeElapsed += QueryIterationPause;
            }
        }

        private static PMTChannelModel GetPMT(ServiceInfoParser parser, PATChannelModel aModel)
        {
            int timeElapsed = 0;
            while (true)
            {
                // Get requested data from MPEG-2 stream
                var result = parser.GetPMTandPAT(aModel);
                // Success - return data
                if (result != null)
                    return result;
                // Check if timeout occured
                if (timeElapsed >= QueryTimeout)
                    return null;
                // Wait some time
                Thread.Sleep(QueryIterationPause);
                timeElapsed += QueryIterationPause;
            }
        }
        #endregion
    }
}
