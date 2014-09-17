using System;
using System.Runtime.InteropServices;
using DirectShowLib;

namespace DVBT2Viewer.BDA.Helpers
{
    #region Some graph tools
    internal class GraphTools
    {
        private GraphTools() { }

        /// <summary>
        /// Get Pin from its type & suptype
        /// </summary>
        /// <param name="aFilter">Target filter</param>
        /// <param name="aPinDirection">Pin direction</param>
        /// <param name="aPinType">Pin type</param>
        /// <param name="aPinSubType">Pin subtype</param>
        /// <returns></returns>
        public static IPin GetPin(IBaseFilter aFilter, PinDirection aPinDirection, Guid aPinType, Guid aPinSubType)
        {
            IEnumPins epins;
            var hr = aFilter.EnumPins(out epins);
            DsError.ThrowExceptionForHR(hr);
            epins.Reset();
            var pins = new IPin[1];
            while (epins.Next(1, pins, IntPtr.Zero) == 0)
            {
                PinInfo pinfo;
                pins[0].QueryPinInfo(out pinfo);
                var direction = pinfo.dir;
                DsUtils.FreePinInfo(pinfo);
                if (direction == aPinDirection)
                {
                    IEnumMediaTypes etypes;
                    hr = pins[0].EnumMediaTypes(out etypes);
                    DsError.ThrowExceptionForHR(hr);
                    etypes.Reset();
                    var mediaTypes = new AMMediaType[1];
                    if (etypes.Next(1, mediaTypes, IntPtr.Zero) == 0)
                    {
                        var done = mediaTypes[0].majorType == aPinType && mediaTypes[0].subType == aPinSubType;
                        DsUtils.FreeAMMediaType(mediaTypes[0]);

                        if (done)
                        {
                            Marshal.ReleaseComObject(etypes);
                            Marshal.ReleaseComObject(epins);
                            return
                                pins[0];
                        }
                    }
                    Marshal.ReleaseComObject(etypes);
                }
                Marshal.ReleaseComObject(pins[0]);
            }
            Marshal.ReleaseComObject(epins);
            return
                null;
        }
    }
    #endregion
}
