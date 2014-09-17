using System;
using System.Runtime.InteropServices;

namespace DVBT2Viewer.BDA.Helpers.HelperTypes
{
    #region KSPROPERTY structure implementation
    [StructLayout(LayoutKind.Sequential)]
    struct KSPROPERTY
    {
        Guid Set;
        [MarshalAs(UnmanagedType.U4)]
        int Id;
        [MarshalAs(UnmanagedType.U4)]
        int Flags;
    }
    #endregion
}
