using System.Runtime.InteropServices;

namespace DVBT2Viewer.BDA.Helpers.HelperTypes
{
    #region KSP_NODE structure implementation
    [StructLayout(LayoutKind.Sequential)]
    struct KSP_NODE
    {
        KSPROPERTY Property;
        [MarshalAs(UnmanagedType.U4)]
        public int NodeId;
        [MarshalAs(UnmanagedType.U4)]
        public int Resevred;
    }
    #endregion
}
