using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

namespace DVBT2Viewer.BDA.Interfaces
{
    #region IMFVideoDisplayControl custom implementation

    [SuppressUnmanagedCodeSecurity]
    [Guid("A490B1E4-AB84-4D31-A1B2-181E03B1077A")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    public interface IMFVideoDisplayControl2
    {
        /// <summary>
        /// Wee neet this to avoid MediaFoundation.NET's method implementation bug 
        /// </summary>
        /// <param name="pszVideo">Video size</param>
        /// <param name="pszARVideo">Video aspect ratio</param>
        /// <returns>HRESULT</returns>
        [MethodImpl(MethodImplOptions.PreserveSig)]
        int GetNativeVideoSize(out Size pszVideo, out Size pszARVideo);
    }
    #endregion
}
