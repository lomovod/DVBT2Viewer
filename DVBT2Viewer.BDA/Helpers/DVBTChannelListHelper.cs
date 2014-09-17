using System.Linq;
using DVBT2Viewer.BDA.Interfaces;
using DVBT2Viewer.BDA.InternalModels;
using DVBT2Viewer.BDA.Models;

namespace DVBT2Viewer.BDA.Helpers
{
    #region DigitalChannels make heplers
    class DVBTChannelListHelper
    {
        private DVBTChannelListHelper() { }

        // Fill SDT data
        public static void ChannelApplySDT(DigitalChannel aChannel, SDTChannelModel aModel)
        {
            aChannel.TSID = aModel.TSID;
            aChannel.ChannelName = aModel.ChannelName;
            aChannel.ProviderName = aModel.ProviderName;
            aChannel.Scrambled = aModel.Scrambled;

            switch (aModel.ChannelType)
            {
                case 1:
                case 22:
                    aChannel.ChannelType = DigitalChannelTypes.TV;
                    break;
                case 2:
                    aChannel.ChannelType = DigitalChannelTypes.Radio;
                    break;
                default:
                    aChannel.ChannelType = DigitalChannelTypes.Unknown;
                    break;
            }
        }

        // Fill PMT PID value
        public static void ChannelApplyPAT(DigitalChannel aChannel, PATChannelModel aModel)
        {
            if (aChannel.SID == aModel.SID)
                aChannel.PMTPID = aModel.PMTPid;
        }

        // Fill PIDs from PMT
        public static void ChannelApplyPMT(DigitalChannel aChannel, PMTChannelModel aModel)
        {
            if (aChannel.SID != aModel.SID) return;
            
            aChannel.PMTPID = aModel.PMTPid;
            foreach (var pmt in aModel.PMTList)
            {
                var channelPmt = aChannel.Streams.Types.FirstOrDefault(x => x.StreamType == pmt.StreamType);
                if (channelPmt == null)
                {
                    channelPmt = new DigitalStreamType { StreamType = pmt.StreamType };
                    aChannel.Streams.Types.Add(channelPmt);
                }
                channelPmt.PID = pmt.PID;
            }
        }
    }
    #endregion
}
