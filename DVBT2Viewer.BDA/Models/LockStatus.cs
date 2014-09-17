
namespace DVBT2Viewer.BDA.Models
{
    #region Tuner lock status
    public class LockStatus
    {
        /// <summary>
        /// Signal strength (level)
        /// </summary>
        public int SignalStrength { get; set; }

        /// <summary>
        /// Signal quality (DVB stream quality)
        /// </summary>
        public int SignalQuality { get; set; }

        /// <summary>
        /// Signal present 
        /// </summary>
        public bool SignalPresent { get; set; }

        /// <summary>
        /// DVB carrier locked
        /// </summary>
        public bool SignalLocked { get; set; }
    }
    #endregion
}
