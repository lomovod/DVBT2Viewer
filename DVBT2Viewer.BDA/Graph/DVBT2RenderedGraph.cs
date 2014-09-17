using System;
using System.Drawing;
using System.Runtime.InteropServices;
using DirectShowLib;
using DirectShowLib.BDA;
using DirectShowLib.Utils;
using DVBT2Viewer.BDA.Helpers;
using DVBT2Viewer.BDA.Interfaces;
using MediaFoundation;
using MediaFoundation.EVR;
using MediaFoundation.Misc;
using AMMediaType = DirectShowLib.AMMediaType;

namespace DVBT2Viewer.BDA.Graph
{
    #region Graph with renderers class
    class DVBT2RenderedGraph : DVBT2BaseGraph
    {
        #region Directshow filters
        protected IBaseFilter videoDecoder = null;
        protected IBaseFilter videoRenderer = null;
        protected IBaseFilter audioDecoder = null;
        protected IBaseFilter audioRenderer = null;
        #endregion

        private int lastUsedVideoPid = -1;  // Last used video PID
        private int lastUsedAudioPid = -1;  // Last used radio PID

        protected IMFVideoDisplayControl displayControl = null;

        /// <summary>
        /// Build directshow graph
        /// </summary>
        /// <param name="renderSurfaceHandle">HWND</param>
        public void BuildGraph(IntPtr renderSurfaceHandle)
        {
            base.BuildGraph();
            BuildVideoAndAudioPins();
            AddVideoFilters();
            AddAudioFilters(); 
            AttachVideoRendererToSurface(renderSurfaceHandle);
        }

        /// <summary>
        /// Set video frame actual size
        /// </summary>
        /// <param name="width">Frame width</param>
        /// <param name="height">Frame height</param>
        public void ResizeVideoFrame(int width, int height)
        {
            if (displayControl == null) return;

            var rcDest = new MFRect();
            var nRect = new MFVideoNormalizedRect(0.0f, 0.0f, 1.0f, 1.0f);

            rcDest.left = 0;
            rcDest.top = 0;
            rcDest.right = width; 
            rcDest.bottom = height;

            var hr = displayControl.SetVideoPosition(nRect, rcDest);
            DsError.ThrowExceptionForHR(hr);
        }

        public void GetVideoFrameSize(out Size size, out Size arSize)
        {
            size = new Size();
            arSize = new Size();
            if (displayControl == null) return;
            var customDisplayControl = (IMFVideoDisplayControl2) displayControl;
            var hr = customDisplayControl.GetNativeVideoSize(out size, out arSize);
            DsError.ThrowExceptionForHR(hr);
        }

        /// <summary>
        /// Force video frame to repaint yourself
        /// </summary>
        public void RepaintVideoFrame()
        {
            if (displayControl == null) return;

            var hr = displayControl.RepaintVideo();
            DsError.ThrowExceptionForHR(hr);
        }

        /// <summary>
        /// Set video PID value to video output pin
        /// </summary>
        /// <param name="pidValue">PID value</param>
        public void SetVideoPid(int pidValue)
        {
            var pid = (IMPEG2PIDMap)DsFindPin.ByName(mpeg2Demux, "Video");
            int hr;
            if (lastUsedVideoPid > 0)
            {
                // Unmap previously mapped PID
                hr = pid.UnmapPID(1, new[] { lastUsedVideoPid });
                DsError.ThrowExceptionForHR(hr);
            }

            if (pidValue > 0)
            {
                // Map new PID
                hr = pid.MapPID(1, new[] { pidValue }, MediaSampleContent.ElementaryStream);
                DsError.ThrowExceptionForHR(hr);
            }
            lastUsedVideoPid = pidValue;
        }

        /// <summary>
        /// Set audio PID value to audio output pin
        /// </summary>
        /// <param name="pidValue">PID value</param>
        public void SetAudioPid(int pidValue)
        {
            var pid = (IMPEG2PIDMap)DsFindPin.ByName(mpeg2Demux, "Audio");
            int hr;
            if (lastUsedAudioPid > 0)
            {
                // Unmap previously mapped PID
                hr = pid.UnmapPID(1, new[] { lastUsedAudioPid });
                DsError.ThrowExceptionForHR(hr);
            }

            if (pidValue > 0)
            {
                // Map new PID
                hr = pid.MapPID(1, new[] { pidValue }, MediaSampleContent.ElementaryStream);
                DsError.ThrowExceptionForHR(hr);
            }
            lastUsedAudioPid = pidValue;
        }

        /// <summary>
        /// Get current video sample
        /// </summary>
        /// <returns></returns>
        public Bitmap GetVideoSample()
        {
            FillVideoDisplayControlInterface();

            var header = new MediaFoundation.Misc.BitmapInfoHeader
            {
                Size = Marshal.SizeOf(typeof(MediaFoundation.Misc.BitmapInfoHeader))
            };

            var buf = IntPtr.Zero;
            try
            {
                var hr = displayControl.RepaintVideo();
                DsError.ThrowExceptionForHR(hr);
                int imageSize;
                long timestamp;

                // Get DIB image & fill BitmapInfoHeader
                hr = displayControl.GetCurrentImage(header, out buf, out imageSize, out timestamp);
                if (hr == 0 && imageSize > 0 && buf != IntPtr.Zero)
                {
                    // Build "Bitmap" instance from BitmapInfoHeader and DIB data
                    return BitmapFromDib.GetBitmapFromDib(header, buf);
                }
                else
                    return null;
            }
            catch
            {
                return null;
            }
            finally
            {
                if (buf != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(buf);
            }
        }

        /// <summary>
        /// Fill IMFVideoDisplayControl (displayControl) value
        /// </summary>
        private void FillVideoDisplayControlInterface()
        {
            if (displayControl != null) return;

            // Get IMFVideoDisplayControl interface
            object o;
            var service = (IMFGetService)videoRenderer;
            service.GetService(MFServices.MR_VIDEO_RENDER_SERVICE, typeof(IMFVideoDisplayControl).GUID,
                out o);
            displayControl = (IMFVideoDisplayControl)o;
        }

        /// <summary>
        /// Attach EVR to HWND to render
        /// </summary>
        /// <param name="renderSurfaceHandle">Renderer surface HWND</param>
        private void AttachVideoRendererToSurface(IntPtr renderSurfaceHandle)
        {
            FillVideoDisplayControlInterface();
            displayControl.SetVideoWindow(renderSurfaceHandle);
            displayControl.SetAspectRatioMode(MFVideoAspectRatioMode.None);
        }

        /// <summary>
        /// Build and setup demuxer video & audio pins
        /// </summary>
        private void BuildVideoAndAudioPins()
        {
            //  Setup video pin media type
            var videoPinType = new AMMediaType
            {
                majorType = MediaType.Video,
                subType = MediaSubType.H264,
                formatType = FormatType.VideoInfo2
            };
            IPin videoPin;
            var hr = ((IMpeg2Demultiplexer)mpeg2Demux).CreateOutputPin(videoPinType, "Video", out videoPin);
            DsError.ThrowExceptionForHR(hr);

            // Setup audio pin media type
            var audioPinType = new AMMediaType
            {
                majorType = MediaType.Audio,
                subType = MediaSubType.Mpeg2Audio,
				sampleSize = 65536,
				temporalCompression = false,
                fixedSizeSamples = true, // or false in MediaPortal //true
				unkPtr = IntPtr.Zero,
                formatType = FormatType.WaveEx
            };

            // We need to set up FormatPtr for proper connection to decoder filter
            var mpeg1WaveFormat = new MPEG1WaveFormat
            {
                wfx = new DirectShowLib.WaveFormatEx
                {
				    wFormatTag = 0x0050,
				    nChannels = 2,
				    nSamplesPerSec = 48000,
				    nAvgBytesPerSec = 32000,
				    nBlockAlign = 768,
				    wBitsPerSample = 0,
				    cbSize = 22 // extra size
                },
				fwHeadLayer = AcmMpegHeadLayer.Layer2,
				//dwHeadBitrate = 0x00177000,
				fwHeadMode = AcmMpegHeadMode.SingleChannel,
				fwHeadModeExt = 1,
				wHeadEmphasis = 1,
				fwHeadFlags = AcmMpegHeadFlags.OriginalHome | AcmMpegHeadFlags.IDMpeg1,
				dwPTSLow = 0,
				dwPTSHigh = 0
            };
			audioPinType.formatSize = Marshal.SizeOf(mpeg1WaveFormat);
			audioPinType.formatPtr = Marshal.AllocHGlobal(audioPinType.formatSize);
            Marshal.StructureToPtr(mpeg1WaveFormat, audioPinType.formatPtr, false);

            IPin audioPin;
            hr = ((IMpeg2Demultiplexer)mpeg2Demux).CreateOutputPin(audioPinType, "Audio", out audioPin);
            DsError.ThrowExceptionForHR(hr);
        }

        /// <summary>
        /// Video filters
        /// </summary>
        private void AddVideoFilters()
        {
            // Microsoft builtin vidoe decoder
            videoDecoder = FilterGraphTools.AddFilterByName(graphBuilder, FilterCategory.LegacyAmFilterCategory, "Microsoft DTV-DVD Video Decoder");
            if (videoDecoder == null)
                throw new Exception("Failed to create video decoder");
            FilterGraphTools.ConnectFilters(graphBuilder, DsFindPin.ByName(mpeg2Demux, "Video"), DsFindPin.ByDirection(videoDecoder, PinDirection.Input, 0), false);

            // Enhanced Video Renderer (EVR)
            videoRenderer = FilterGraphTools.AddFilterFromClsid(graphBuilder, typeof(EnhancedVideoRenderer).GUID, "Enhanced Video Renderer");
            FilterGraphTools.ConnectFilters(graphBuilder, DsFindPin.ByDirection(videoDecoder, PinDirection.Output, 0), DsFindPin.ByDirection(videoRenderer, PinDirection.Input, 0), false);

            var service = (IEVRFilterConfig)videoRenderer;
            service.SetNumberOfStreams(1);
        }

        private void AddAudioFilters()
        {
            // Microsoft buitin audio decoder
            audioDecoder = FilterGraphTools.AddFilterByName(graphBuilder, FilterCategory.LegacyAmFilterCategory, "Microsoft DTV-DVD Audio Decoder");
            if (audioDecoder == null)
                throw new Exception("Failed to create audio decoder");
            FilterGraphTools.ConnectFilters(graphBuilder, DsFindPin.ByName(mpeg2Demux, "Audio"), DsFindPin.ByDirection(audioDecoder, PinDirection.Input, 0), true);

            // Standard DirectSound output
            audioRenderer = FilterGraphTools.AddFilterFromClsid(graphBuilder, typeof(DSoundRender).GUID, "Default DirectSound Device");
            FilterGraphTools.ConnectFilters(graphBuilder, DsFindPin.ByDirection(audioDecoder, PinDirection.Output, 0), DsFindPin.ByDirection(audioRenderer, PinDirection.Input, 0), false);
        }
    }
    #endregion
}
