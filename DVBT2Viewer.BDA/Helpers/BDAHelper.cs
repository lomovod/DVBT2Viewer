using DVBT2Viewer.BDA.Interfaces;

namespace DVBT2Viewer.BDA.Helpers
{
    #region Public DVB-T/T2 classes factory
    public class BDAHelper
    {
        private BDAHelper() { }

        /// <summary>
        /// Get channel finder
        /// </summary>
        /// <returns>Channel finder interface</returns>
        public static IDVBT2Finder GetFinder()
        {
            return new DVBT2Finder();
        }

        /// <summary>
        /// Get channel renderer
        /// </summary>
        /// <returns>Channel renderer interface</returns>
        public static IDVBT2Renderer GetRenderer()
        {
            return new DVBT2Renderer();
        }
    }
    #endregion
}
