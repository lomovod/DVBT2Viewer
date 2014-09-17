using System;
using DVBT2Viewer.BDA.Models;

namespace DVBT2Viewer.BDA.Interfaces
{
    #region DVB-T2 base operations

    public interface IDVBT2Base : IDisposable
    {
        /// <summary>
        /// Get DVB-T/T2 tuner device name
        /// </summary>
        string TunerDeviceName { get; }

        /// <summary>
        /// Run DirectShow graph
        /// </summary>
        void Run();

        /// <summary>
        /// Stop DirectShow Graph
        /// </summary>
        void Stop();

        /// <summary>
        /// Get tune lock params
        /// </summary>
        /// <returns></returns>
        LockStatus GetLockStatus();
    }
    #endregion
}
