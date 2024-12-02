using System;
using System.Runtime.InteropServices;
namespace OPCAE.OPC.Common
{

    [Guid("7AA83A01-6C77-11d3-84F9-00008630A38B"), ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IOPCSecurityNT
    {
        [PreserveSig]
        int IsAvailableNT([MarshalAs(UnmanagedType.Bool)] out bool pbAvailable);
        [PreserveSig]
        int QueryMinImpersonationLevel(out int pdwMinImpLevel);
        [PreserveSig]
        int ChangeUser();
    }
}

