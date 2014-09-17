
namespace DVBT2Viewer.BDA.Interfaces
{
    #region Degital channel types
    public enum DigitalChannelTypes
    {
        Unknown,    // Type unknown
        TV,         // DVB TV
        Radio       // DVB Radio
    }
    #endregion

    #region Selectable digital channel interface
    public interface IDigitalChannel
    {
        /// <summary>
        /// Frequency in kHz
        /// </summary>
        int Frequency { get; }

        /// <summary>
        /// Bandwidth in MHz
        /// </summary>
        int Bandwidth { get; }

        /// <summary>
        /// Channel SID
        /// </summary>
        short SID { get; }

        /// <summary>
        /// Channel H264 video PID (TV only)
        /// </summary>
        short VideoH264Pid { get; }

        /// <summary>
        /// Channel MPA audio PID (TV & Radio)
        /// </summary>
        short AudioMPAPid { get; }

        /// <summary>
        /// Channel type
        /// </summary>
        DigitalChannelTypes ChannelType { get; }

        /// <summary>
        /// Stream ID (PLP ID)
        /// </summary>
        int StreamId { get; }
    }
    #endregion
}
