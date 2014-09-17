
namespace DVBT2Viewer.UI.Interfaces
{
    #region Closable window
    interface IClosableWindowActions
    {
        /// <summary>
        /// Close the window
        /// </summary>
        /// <param name="closeValue">True if need to save state</param>
        void Close(bool closeValue);
    }
    #endregion
}
