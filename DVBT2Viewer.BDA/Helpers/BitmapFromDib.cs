using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;

namespace DVBT2Viewer.BDA.Helpers
{
    #region Header of bitmap file
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct BITMAPFILEHEADER
    {
        public Int16 bfType;
        public Int32 bfSize;
        public Int16 bfReserved1;
        public Int16 bfReserved2;
        public Int32 bfOffBits;
    };
    #endregion

    #region Bitmap from EVR DIB blob
    internal class BitmapFromDib
    {
        private BitmapFromDib() { }

        internal static byte[] GetBitmapDataFromDib(MediaFoundation.Misc.BitmapInfoHeader aInfoHeader, IntPtr aData)
        {
            using (var stream = new MemoryStream())
            {
                // Make BMP file header
                var fileHeader = new BITMAPFILEHEADER
                {
                    bfType = 0x4d42,
                    bfSize =
                        aInfoHeader.ImageSize + Marshal.SizeOf(typeof(BITMAPFILEHEADER)) +
                        Marshal.SizeOf(typeof(MediaFoundation.Misc.BitmapInfoHeader)),
                    bfOffBits =
                        Marshal.SizeOf(typeof(BITMAPFILEHEADER)) +
                        Marshal.SizeOf(typeof(MediaFoundation.Misc.BitmapInfoHeader))
                };

                // Write file header to stream
                var fileHeaderPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(BITMAPFILEHEADER)));
                Marshal.StructureToPtr(fileHeader, fileHeaderPtr, true);
                var fileHeaderBytes = new byte[Marshal.SizeOf(typeof(BITMAPFILEHEADER))];
                Marshal.Copy(fileHeaderPtr, fileHeaderBytes, 0, fileHeaderBytes.Length);
                Marshal.FreeHGlobal(fileHeaderPtr);
                stream.Write(fileHeaderBytes, 0, fileHeaderBytes.Length);

                // Write BMP Info header to stream
                var biHeaderBytes = new byte[Marshal.SizeOf(typeof(MediaFoundation.Misc.BitmapInfoHeader))];
                Marshal.Copy(aInfoHeader.GetPtr(), biHeaderBytes, 0, biHeaderBytes.Length);
                stream.Write(biHeaderBytes, 0, biHeaderBytes.Length);

                // Write data (DIB) to stream
                var dataBytes = new byte[aInfoHeader.ImageSize];
                Marshal.Copy(aData, dataBytes, 0, aInfoHeader.ImageSize);
                stream.Write(dataBytes, 0, dataBytes.Length);

                // File header + Info header + DIB data = bitmap
                return stream.ToArray();
            }

        }

        internal static Bitmap GetBitmapFromDib(MediaFoundation.Misc.BitmapInfoHeader aInfoHeader, IntPtr aData)
        {
            // This code remains from dark "SampleGrabber" ages
            using (var stream = new MemoryStream(GetBitmapDataFromDib(aInfoHeader, aData)))
            {
                stream.Position = 0;
                var bitmap = new Bitmap(stream);
                var bitmap2 = new Bitmap(bitmap);
                bitmap.Dispose();
                return bitmap2;
            }
        }
    }
    #endregion
}
