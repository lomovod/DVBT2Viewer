using System;
using System.Runtime.InteropServices;
using DirectShowLib;
using DVBT2Viewer.BDA.Helpers.HelperTypes;

namespace DVBT2Viewer.BDA.Helpers
{
    #region KSP_NODE query functions
    class KsPropertyHelper
    {
        private KsPropertyHelper() { }

        /// <summary>
        /// GET query
        /// </summary>
        /// <param name="ksTarget">Target node</param>
        /// <param name="ksGuid">Query GUID</param>
        /// <param name="ksParam">Query param code</param>
        /// <param name="ksType">Query return data type</param>
        /// <returns>Query result</returns>
        public static object KSGetNode(IKsPropertySet ksTarget, Guid ksGuid, int ksParam, Type ksType)
        {
            object obj;

            var dataPtrSize = Marshal.SizeOf(ksType);
            var dataPtr = Marshal.AllocCoTaskMem(dataPtrSize);
            var instancePtrSize = Marshal.SizeOf(typeof(KSP_NODE));
            var instancePtr = Marshal.AllocCoTaskMem(instancePtrSize);

            try
            {
                int cbBytes;
                var result = ksTarget.Get(
                    ksGuid,
                    ksParam,
                    instancePtr,
                    instancePtrSize,
                    dataPtr,
                    dataPtrSize,
                    out cbBytes);

                if (result != 0)
                    throw new Exception(string.Format("KSPropertySet KSP_NODE GET method failed [{0:X}]", result));
                obj = Marshal.PtrToStructure(dataPtr, ksType);
            }
            finally
            {
                if (dataPtr != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(dataPtr);
                if (instancePtr != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(instancePtr);
            }

            return obj;
        }

        /// <summary>
        /// SET query
        /// </summary>
        /// <param name="ksTarget">Query target</param>
        /// <param name="ksGuid">Query GUID</param>
        /// <param name="ksParam">Query param code</param>
        /// <param name="ksStructure">Query param value</param>
        public static void KSSetNode(IKsPropertySet ksTarget, Guid ksGuid, int ksParam, object ksStructure)
        {
            var dataPtrSize = Marshal.SizeOf(ksStructure);
            var dataPtr = Marshal.AllocCoTaskMem(dataPtrSize);
            var instancePtrSize = Marshal.SizeOf(typeof(KSP_NODE));
            var instancePtr = Marshal.AllocCoTaskMem(instancePtrSize);

            Marshal.StructureToPtr(ksStructure, dataPtr, true);

            try
            {
                var result = ksTarget.Set(
                    ksGuid,
                    ksParam,
                    instancePtr,
                    instancePtrSize,
                    dataPtr,
                    dataPtrSize);
                if (result != 0)
                    throw new Exception(string.Format("KSPropertySet KSP_NODE SET method failed [{0:X}]", result));
            }
            finally
            {
                if (dataPtr != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(dataPtr);
                if (instancePtr != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(instancePtr);
            }
        }

        /// <summary>
        /// Check if query supported
        /// </summary>
        /// <param name="propSet">Query target to check</param>
        /// <param name="ksGuid">Query GUID to check</param>
        /// <param name="ksParam">Query param code to check</param>
        /// <returns>KSPropertySupport structure containing info about GET and SET support</returns>
        public static KSPropertySupport KSSupported(IKsPropertySet propSet, Guid ksGuid, int ksParam)
        {
            KSPropertySupport dwSupported;
            var hr = propSet.QuerySupported(ksGuid, ksParam, out dwSupported);
            if (hr != 0)
                dwSupported = 0;
            return
                dwSupported;
        }
    }
    #endregion
}
