using DVBT2Viewer.BDA.Models;

namespace DVBT2Viewer.BDA.Interfaces
{
    #region DVB-T2 channel finder public interface
    public interface IDVBT2Finder : IDVBT2Base
    {
        /// <summary>
        /// Build DVB-T/T2 graph
        /// </summary>
        void Build();

        /// <summary>
        /// Lock tuner 
        /// </summary>
        /// <param name="frequency">Frequency to lock (kHz)</param>
        /// <param name="bandwidth">Bandwidth to lock (MHz)</param>
        void Lock(int frequency, int bandwidth);

        /// <summary>
        /// Get channels info 
        /// </summary>
        /// <param name="frequency">Frequency to query info (kHz)</param>
        /// <param name="bandwidth">Bandwidth to query info (MHz)</param>
        /// <returns></returns>
        DigitalMultiplex GetMultiplex(int frequency, int bandwidth);
    }
    #endregion
}
