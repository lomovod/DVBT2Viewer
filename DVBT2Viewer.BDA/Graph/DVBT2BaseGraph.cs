using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using DirectShowLib;
using DirectShowLib.BDA;
using DirectShowLib.Utils;
using DVBT2Viewer.BDA.Helpers;
using DVBT2Viewer.BDA.Interfaces;
using DVBT2Viewer.BDA.Models;

namespace DVBT2Viewer.BDA.Graph
{
    #region Base graph class
    class DVBT2BaseGraph : IDVBT2BaseGraphProps, IDisposable
    {
        //private DsROTEntry rot = null;

        protected IFilterGraph2 graphBuilder = null;

        #region Directshow Filters
        protected IBaseFilter networkProvider = null;
        protected IBaseFilter mpeg2Demux = null;
        protected IBaseFilter tuner = null;
        protected IBaseFilter capture = null;
        protected IBaseFilter bdaTIF = null;
        protected IBaseFilter bdaSectTables = null;
        #endregion

        /// <summary>
        /// Device name
        /// </summary>
        public string TunerDeviceName { get; private set; }

        /// <summary>
        /// Tuner filter output pin
        /// </summary>
        public IKsPropertySet TunerPin { get; private set; }

        /// <summary>
        /// DVB-T tuning space
        /// </summary>
        public ITuningSpace TuningSpace { get { return GetTuningSpace(); } }

        public DVBT2BaseGraph()
        {
            // Create FilterGraph
            graphBuilder = (IFilterGraph2)new FilterGraph();

            //rot = new DsROTEntry(graphBuilder);
        }

        /// <summary>
        /// Build Directshow graph
        /// </summary>
        public virtual void BuildGraph()
        {
            AddNetworkProvider();
            ApplyTuningSpace();
            AddTunerAndCaptureFilters();
            AddDemuxerFilters();
        }

        #region Run & stop
        public void Run()
        {
            var hr = ((IMediaControl) graphBuilder).Run();
            DsError.ThrowExceptionForHR(hr);
        }

        public void Stop()
        {
            var hr = ((IMediaControl) graphBuilder).Stop();
            DsError.ThrowExceptionForHR(hr);
        }
        #endregion

        /// <summary>
        /// Tune
        /// </summary>
        /// <param name="aTuneRequest">ITuneRequest for channel tuning</param>
        public void Tune(ITuneRequest aTuneRequest)
        {
            var hr = ((ITuner) networkProvider).put_TuneRequest(aTuneRequest);
            DsError.ThrowExceptionForHR(hr);
        }

        /// <summary>
        /// Common GetLockStatus implementation
        /// </summary>
        /// <returns></returns>
        public LockStatus GetLockStatus()
        {
            // Get IBDA_Topology interface
            var topology = (IBDA_Topology)tuner;

            int nodeTypesCount;
            var nodeType = new int[64];
            var ifs = new Guid[64];

            // Get topoloye node types count
            topology.GetNodeTypes(out nodeTypesCount, 64, nodeType);
            for (var i = 0; i < nodeTypesCount; i++)
            {
                int interfaces;
                // Get node interfaces
                topology.GetNodeInterfaces(nodeType[i], out interfaces, 64, ifs);
                for (var j = 0; j < interfaces; j++)
                {
                    // Check for IBDA_SignalStatistics interface
                    if (ifs[j] == typeof(IBDA_SignalStatistics).GUID)
                    {
                        // Success - extracting interface 
                        object ostats;
                        topology.GetControlNode(0, 1, nodeType[i], out ostats);
                        var stats = (IBDA_SignalStatistics)ostats;

                        int strength;
                        int quality;
                        bool present;
                        bool locked;

                        int hr = 0;

                        hr += stats.get_SignalStrength(out strength);
                        hr += stats.get_SignalQuality(out quality);
                        hr += stats.get_SignalPresent(out present);
                        hr += stats.get_SignalLocked(out locked);

                        if (hr != 0)
                            throw new Exception("Cannot find suitable interface");

                        return new LockStatus
                        {
                            SignalStrength = strength,
                            SignalQuality = quality,
                            SignalLocked = locked,
                            SignalPresent = present
                        };
                    }
                }
            }

            throw new Exception("Cannot get tuner lock status");
        }

        /// <summary>
        /// Get service info parser 
        /// </summary>
        /// <returns>Service info parser instance</returns>
        public ServiceInfoParser GetServiceInfoParser()
        {
            return new ServiceInfoParser(bdaSectTables as IMpeg2Data);
        }

        /// <summary>
        /// Get Tuner ouptut pin node manager
        /// </summary>
        /// <returns>Node manager instance</returns>
        public TunerKspNodeManager GetTunerNodeManager()
        {
            return new TunerKspNodeManager(TunerPin);
        }

        public void Dispose()
        {
            ((IMediaControl) graphBuilder).StopWhenReady();
            ((IMediaControl) graphBuilder).Stop();

            FilterGraphTools.DisconnectAllPins(graphBuilder);
            FilterGraphTools.RemoveAllFilters(graphBuilder);

            if (graphBuilder != null)
            {
                Marshal.ReleaseComObject(graphBuilder);
                graphBuilder = null;
            }
        }

        #region Add base filters
        private void AddNetworkProvider()
        {
            //networkProvider = FilterGraphTools.AddFilterFromClsid(graphBuilder, new Guid("{B2F3A67C-29DA-4C78-8831-091ED509A475}"), "Microsoft Network Provider");

            Guid networkProviderClsid;
            var hr = TuningSpace.get__NetworkType(out networkProviderClsid);
            DsError.ThrowExceptionForHR(hr);
            networkProvider = FilterGraphTools.AddFilterFromClsid(graphBuilder, networkProviderClsid, "Microsoft DVB-T Network Provider");
            if (networkProvider == null)
                throw new Exception("DVB-T Network provider is not installed");
        }

        private void ApplyTuningSpace()
        {
            Debug.Assert(networkProvider != null, "networkProvider != null");
            var hr = ((ITuner)networkProvider).put_TuningSpace(TuningSpace);
            DsError.ThrowExceptionForHR(hr);
        }

        private void AddTunerAndCaptureFilters()
        {
            // Provider -> Tuner
            AddTunerFilterDependOnNetworkProvider();

            // Tuner -> Capture
            AddCaptureFilterDependOnTunerFilter();

            // Store tuner pin (for IKsPropertySet requests)
            TunerPin = DsFindPin.ByDirection(tuner, PinDirection.Output, 0) as IKsPropertySet;
        }

        private void AddDemuxerFilters()
        {
            // Add MPEG-2 Demultiplexer
            mpeg2Demux = FilterGraphTools.AddFilterFromClsid(graphBuilder, typeof(MPEG2Demultiplexer).GUID, "MPEG-2 Demultiplexer");
            if (mpeg2Demux == null)
                throw new Exception("Failed to create demuxer");
            FilterGraphTools.ConnectFilters(graphBuilder, DsFindPin.ByDirection(capture, PinDirection.Output, 0), DsFindPin.ByDirection(mpeg2Demux, PinDirection.Input, 0), false);

            // Add Sections and Table filter
            bdaSectTables = FilterGraphTools.AddFilterByName(graphBuilder, FilterCategory.BDATransportInformationRenderersCategory, "MPEG-2 Sections and Tables");
            if (bdaSectTables == null)
                throw new Exception("Failed to create Sections and tables");
            FilterGraphTools.ConnectFilters(graphBuilder, GraphTools.GetPin(mpeg2Demux, PinDirection.Output, MediaType.Mpeg2Sections, MediaSubType.Mpeg2Data),
                DsFindPin.ByDirection(bdaSectTables, PinDirection.Input, 0), false);

            // Add Transport Informantion filter
            bdaTIF = FilterGraphTools.AddFilterByName(graphBuilder, FilterCategory.BDATransportInformationRenderersCategory, "BDA MPEG2 Transport Information Filter");
            if (bdaTIF == null)
                throw new Exception("Failed to create TIF");
            FilterGraphTools.ConnectFilters(graphBuilder, GraphTools.GetPin(mpeg2Demux, PinDirection.Output, MediaType.Mpeg2Sections, MediaSubType.DvbSI),
                DsFindPin.ByDirection(bdaTIF, PinDirection.Input, 0), false);

            // Remove all auto-created (and still not used) MPEG-2 Demultiplexer output pins
            ClearDemuxerPins();
        }
        #endregion

        #region clear all not used demuxer pins
        private void ClearDemuxerPins()
        {
            var pins = new IPin[1];
            IEnumPins enumPins;

            var hr = mpeg2Demux.EnumPins(out enumPins);
            DsError.ThrowExceptionForHR(hr);
            try
            {
                while (enumPins.Next(1, pins, IntPtr.Zero) == 0)
                {
                    PinInfo pinInfo;
                    hr = pins[0].QueryPinInfo(out pinInfo);
                    DsError.ThrowExceptionForHR(hr);

                    // Is it the right direction?
                    if (pinInfo.dir == PinDirection.Output)
                    {
                        IPin targetPin;
                        hr = pins[0].ConnectedTo(out targetPin);
                        if (hr != 0)
                            ((IMpeg2Demultiplexer)mpeg2Demux).DeleteOutputPin(pinInfo.name);
                        else
                            if (targetPin != null)
                                Marshal.ReleaseComObject(targetPin);
                    }
                    Marshal.ReleaseComObject(pins[0]);
                }
            }
            finally
            {
                Marshal.ReleaseComObject(enumPins);
            }
        }
        #endregion

        /// <summary>
        /// Add "DVB-T provider filter" -> "Tuner filter" link
        /// </summary>
        private void AddTunerFilterDependOnNetworkProvider()
        {
            // Enum all available BDA source filters
            foreach (var device in DsDevice.GetDevicesOfCat(FilterCategory.BDASourceFiltersCategory))
            {
                try
                {
                    // Add filter to graph
                    tuner = FilterGraphTools.AddFilterByName(graphBuilder, FilterCategory.BDASourceFiltersCategory,
                        device.Name);
                    if (tuner == null)
                        throw new Exception("Failed to create tuner filter");

                    // Try to connect source filter to network provider filter
                    FilterGraphTools.ConnectFilters(graphBuilder, DsFindPin.ByDirection(networkProvider, PinDirection.Output, 0),
                        DsFindPin.ByDirection(tuner, PinDirection.Input, 0), false);

                    TunerDeviceName = device.Name;
                }
                catch
                {
                    // Connection failed
                    if (capture != null)
                    {
                        // Remove filter from graph
                        graphBuilder.RemoveFilter(capture);
                        // Release filter's COM object
                        Marshal.ReleaseComObject(capture);
                        capture = null;
                    }                    
                    throw;
                }
                if (tuner == null)
                    throw new Exception("Cannot create capture filter");
            }
        }

        /// <summary>
        /// Add "Tuner filter" -> "Capture filter" link
        /// </summary>
        private void AddCaptureFilterDependOnTunerFilter()
        {
            foreach (var device in DsDevice.GetDevicesOfCat(FilterCategory.BDAReceiverComponentsCategory))
            {
                try
                {
                    // Add filter to graph
                    capture = FilterGraphTools.AddFilterByName(graphBuilder, FilterCategory.BDAReceiverComponentsCategory,
                        device.Name);
                    if (capture == null)
                        throw new Exception("Failed to create capture filter");

                    // Try to connect filters
                    FilterGraphTools.ConnectFilters(graphBuilder, DsFindPin.ByDirection(tuner, PinDirection.Output, 0),
                        DsFindPin.ByDirection(capture, PinDirection.Input, 0), false);
                }
                catch
                {
                    if (capture != null)
                    {
                        // Remove filter from graph
                        graphBuilder.RemoveFilter(capture);
                        // Release filter's COM object
                        Marshal.ReleaseComObject(capture);
                        capture = null;
                    }
                }
            }
            if (capture == null)
                throw new Exception("Cannot create capture filter");
        }

        /// <summary>
        /// Build tuning space
        /// </summary>
        /// <returns></returns>
        private static ITuningSpace GetTuningSpace()
        {
            // Create DVB-T tuning space
            var tuningSpace = (IDVBTuningSpace)new DVBTuningSpace();
            var hr = tuningSpace.put_UniqueName("DVBT TuningSpace");
            DsError.ThrowExceptionForHR(hr);
            hr = tuningSpace.put_FriendlyName("DVBT TuningSpace");
            DsError.ThrowExceptionForHR(hr);
            hr = tuningSpace.put__NetworkType(typeof(DVBTNetworkProvider).GUID);
            DsError.ThrowExceptionForHR(hr);
            hr = tuningSpace.put_SystemType(DVBSystemType.Terrestrial);
            DsError.ThrowExceptionForHR(hr);
            
            // Make TuneRequest
            ITuneRequest tr;
            hr = tuningSpace.CreateTuneRequest(out tr);
            DsError.ThrowExceptionForHR(hr);

            // Fill TuneRequest with defaults
            var tuneRequest = (IDVBTuneRequest)tr;
            hr = tuneRequest.put_ONID(-1);
            DsError.ThrowExceptionForHR(hr);
            hr = tuneRequest.put_TSID(-1);
            DsError.ThrowExceptionForHR(hr);
            hr = tuneRequest.put_SID(-1);
            DsError.ThrowExceptionForHR(hr);

            // Make DVB-T locator & fill it with defaults
            var locator = (IDVBTLocator)new DVBTLocator();
            hr = locator.put_CarrierFrequency(-1);
            DsError.ThrowExceptionForHR(hr);
            hr = locator.put_Bandwidth(-1);
            DsError.ThrowExceptionForHR(hr);
            hr = tr.put_Locator(locator);
            DsError.ThrowExceptionForHR(hr);
            return tuningSpace;
        }

    }
    #endregion
}
