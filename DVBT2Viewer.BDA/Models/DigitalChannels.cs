using System.Collections.Generic;
using System.Linq;
using DVBT2Viewer.BDA.Interfaces;

namespace DVBT2Viewer.BDA.Models
{
    #region Stream type
    public class DigitalStreamType
    {
        /// <summary>
        /// "Stream type" value
        /// </summary>
        public byte StreamType { get; set; }

        /// <summary>
        /// PID value
        /// </summary>
        public short PID { get; set; }
    }
    #endregion

    #region Channel stream types collection
    public class DigitalStreamTypes
    {
        private readonly IList<DigitalStreamType> types = new List<DigitalStreamType>();

        /// <summary>
        /// List of stream types
        /// </summary>
        public IList<DigitalStreamType> Types { get { return types; } }

        /// <summary>
        /// H264 video PID value
        /// </summary>
        public DigitalStreamType VideoH264 { get { return types.FirstOrDefault(x => x.StreamType == 0x1B); } }

        /// <summary>
        /// MPA audio PID value
        /// </summary>
        public DigitalStreamType AudioMPA { get { return types.FirstOrDefault(x => x.StreamType == 0x03); } }

        /// <summary>
        /// Teletext PID value
        /// </summary>
        public DigitalStreamType Teletext { get { return types.FirstOrDefault(x => x.StreamType == 0x06); } }
    }
    #endregion

    #region Digital Channel
    public class DigitalChannel : IDigitalChannel
    {
        private readonly DigitalStreamTypes streams = new DigitalStreamTypes();
        private readonly DigitalMultiplex parentMultiplex;

        /// <summary>
        /// Channel name
        /// </summary>
        public string ChannelName { get; set; } 

        /// <summary>
        /// Provider name
        /// </summary>
        public string ProviderName { get; set; } 

        /// <summary>
        /// SID
        /// </summary>
        public short SID { get; set; } 

        /// <summary>
        /// TSID
        /// </summary>
        public short TSID { get; set; } 

        /// <summary>
        /// PMT PID
        /// </summary>
        public short PMTPID { get; set; } 

        /// <summary>
        /// is CA encryption present?
        /// </summary>
        public bool Scrambled { get; set; } 

        /// <summary>
        /// Stream (PLP) ID
        /// </summary>
        public int StreamId { get; set; } 

        /// <summary>
        /// TV / Radio / Other
        /// </summary>
        public DigitalChannelTypes ChannelType { get; set; } 

        /// <summary>
        /// Channel PID streams
        /// </summary>
        public DigitalStreamTypes Streams { get { return streams; } } 

        /// <summary>
        /// Multiplex which channel belong to
        /// </summary>
        public DigitalMultiplex ParentMultiplex { get { return parentMultiplex; } } 

        /// <summary>
        /// H264 PID (IDigitalChannel inplementation)
        /// </summary>
        public short VideoH264Pid { get { return streams != null && streams.VideoH264 != null ? streams.VideoH264.PID : (short) -1; } }

        /// <summary>
        /// Audio PID (IDigitalChannel inplementation)
        /// </summary>
        public short AudioMPAPid { get { return streams != null && streams.AudioMPA != null ? streams.AudioMPA.PID : (short) -1; } }

        /// <summary>
        /// Channel carrier frequency (IDigitalChannel inplementation)
        /// </summary>
        public int Frequency { get { return parentMultiplex != null ? parentMultiplex.Frequency : -1; } }

        /// <summary>
        /// Channel carrier bandwidth (IDigitalChannel inplementation)
        /// </summary>
        public int Bandwidth { get { return parentMultiplex != null ? parentMultiplex.Bandwidth : -1; } }

        internal DigitalChannel(DigitalMultiplex multiplex)
        {
            parentMultiplex = multiplex;
            StreamId = -1;  // -1 means "dont try to switch the stream"
        }

    }
    #endregion

    #region Multiplex
    public class DigitalMultiplex
    {
        private readonly IList<DigitalChannel> channels = new List<DigitalChannel>();

        /// <summary>
        /// Multiplex frequency
        /// </summary>
        public int Frequency { get; set; } // Frequency in kHz

        /// <summary>
        /// Multiplex bandwidth
        /// </summary>
        public int Bandwidth { get; set; } // Bandwidth in MHz

        /// <summary>
        /// Channels
        /// </summary>
        public IList<DigitalChannel> Channels { get { return channels; } } // Channels

        public DigitalChannel CreateChannel()
        {
            return new DigitalChannel(this);
        }
    }
    #endregion
}
