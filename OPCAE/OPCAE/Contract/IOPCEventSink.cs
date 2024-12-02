using System;
using System.Runtime.InteropServices;

namespace OPCAE.OPCAE.Contract
{
 
    [ComImport, ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("6516885F-5783-11D1-84A0-00608CB8A7E9")]
    public interface IOPCEventSink
    {
        [PreserveSig]
        int OnEvent([In] int hClientSubscription, [In, MarshalAs(UnmanagedType.Bool)] bool bRefresh, [In, MarshalAs(UnmanagedType.Bool)] bool bLastRefresh, [In] int dwCount, [In] IntPtr pEvents);
    }
}

