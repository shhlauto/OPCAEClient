using System;
using System.Runtime.InteropServices;
namespace OPCAE.OPC.Common
{

    [ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("7AA83A02-6C77-11d3-84F9-00008630A38B")]
    public interface IOPCSecurityPrivate
    {
        [PreserveSig]
        int IsAvailablePriv([MarshalAs(UnmanagedType.Bool)] out bool pbAvailable);
        [PreserveSig]
        int Logon([In, MarshalAs(UnmanagedType.LPWStr)] string szUserID, [In, MarshalAs(UnmanagedType.LPWStr)] string szPassword);
        [PreserveSig]
        int Logoff();
    }
}

