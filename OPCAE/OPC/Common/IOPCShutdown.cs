using System;
using System.Runtime.InteropServices;
namespace OPCAE.OPC.Common
{

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), ComVisible(true), Guid("F31DFDE1-07B6-11d2-B2D8-0060083BA1FB")]
    public interface IOPCShutdown
    {
        void ShutdownRequest([In, MarshalAs(UnmanagedType.LPWStr)] string szReason);
    }
}

