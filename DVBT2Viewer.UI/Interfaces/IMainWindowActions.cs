using DVBT2Viewer.BDA.Models;

namespace DVBT2Viewer.UI.Interfaces
{
    #region MainWindow actions interface
    interface IMainWindowActions : IClosableWindowActions
    {
        /// <summary>
        /// Invoke search channels interface
        /// </summary>
        /// <returns>Search result</returns>
        DigitalMultiplex SearchChannels();
    }
    #endregion
}
