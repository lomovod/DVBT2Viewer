using System;
using System.Drawing;

namespace DVBT2Viewer.BDA.Interfaces
{
    #region DVB-T2 channel renderer public interface
    public interface IDVBT2Renderer : IDVBT2Base
    {
        /// <summary>
        /// Build DVB-T/T2 graph
        /// </summary>
        /// <param name="renderSurfaceHandle">HWND of surface to render video content</param>
        void Build(IntPtr renderSurfaceHandle);

        /// <summary>
        /// Select channel to render
        /// </summary>
        /// <param name="channel">Channel</param>
        void SelectChannel(IDigitalChannel channel);

        /// <summary>
        /// Get rendered video sample (screenshot)
        /// </summary>
        /// <returns></returns>
        Bitmap GetVideoSample();

        /// <summary>
        /// Set rendered video frame size
        /// </summary>
        /// <param name="width">Width in pixels</param>
        /// <param name="height">Heigth in pixels</param>
        void ResizeVideoFrame(int width, int height);

        /// <summary>
        /// Force rendered video frame to repaint
        /// </summary>
        void RepaintVideoFrame();

        void GetVideoFrameSize(out Size size, out Size arSize);
    }
    #endregion
}
