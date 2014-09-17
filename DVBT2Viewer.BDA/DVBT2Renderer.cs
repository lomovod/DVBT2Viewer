using System;
using System.Drawing;
using DirectShowLib;
using DirectShowLib.BDA;
using DVBT2Viewer.BDA.Graph;
using DVBT2Viewer.BDA.Interfaces;
using DVBT2Viewer.BDA.Models;

namespace DVBT2Viewer.BDA
{
    internal class DVBT2Renderer : DVBT2BaseGraphCommands, IDVBT2Renderer
    {
        private readonly DVBT2RenderedGraph graph = new DVBT2RenderedGraph();

        public string TunerDeviceName { get { return graph.TunerDeviceName; } }

        public void Dispose()
        {
            graph.Dispose();
        }

        public void Run()
        {
            graph.Run();
        }

        public void Stop()
        {
            graph.Stop();
        }

        public LockStatus GetLockStatus()
        {
            return graph.GetLockStatus();
        }

        public void Build(IntPtr renderSurfaceHandle)
        {
            graph.BuildGraph(renderSurfaceHandle);
        }

        public void SelectChannel(IDigitalChannel channel)
        {
            if (channel.StreamId > -1)
                SetPlp(channel.StreamId);
            var tuneRequest = GetTuneRequest(channel.Frequency, channel.Bandwidth);
            ((IDVBTuneRequest)tuneRequest).put_SID(channel.SID);
            ((IDVBTuneRequest)tuneRequest).put_TSID(-1);
            ((IDVBTuneRequest)tuneRequest).put_ONID(-1);


            graph.SetVideoPid(channel.VideoH264Pid);
            graph.SetAudioPid(channel.AudioMPAPid);

            graph.Tune(tuneRequest);
        }

        public Bitmap GetVideoSample()
        {
            return graph.GetVideoSample();
        }

        public void ResizeVideoFrame(int width, int height)
        {
            graph.ResizeVideoFrame(width, height);
        }

        public void RepaintVideoFrame()
        {
            graph.RepaintVideoFrame();
        }

        public void GetVideoFrameSize(out Size size, out Size arSize)
        {
            graph.GetVideoFrameSize(out size, out arSize);
        }

        protected override IDVBT2BaseGraphProps GetGraph()
        {
            return graph;
        }
    }
}
