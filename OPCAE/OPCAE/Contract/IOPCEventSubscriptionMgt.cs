using System;
using System.Runtime.InteropServices;

namespace OPCAE.OPCAE.Contract
{
   
    [ComImport, ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("65168855-5783-11D1-84A0-00608CB8A7E9")]
    public interface IOPCEventSubscriptionMgt
    {
        [PreserveSig]
        int SetFilter([In] int dwEventType, [In] int dwNumCategories, [In, MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.U4, SizeParamIndex=1)] int[] pdwEventCategories, [In] int dwLowSeverity, [In] int dwHighSeverity, [In] int dwNumAreas, [In, MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.LPWStr, SizeParamIndex=5)] string[] pszAreaList, [In] int dwNumSources, [In, MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.LPWStr, SizeParamIndex=7)] string[] pszSourceList);
        [PreserveSig]
        int GetFilter(out int pdwEventType, out int pdwNumCategories, out IntPtr ppdwEventCategories, out int pdwLowSeverity, out int pdwHighSeverity, out int pdwNumAreas, out IntPtr ppszAreaList, out int pdwNumSources, out IntPtr ppszSourceList);
        [PreserveSig]
        int SelectReturnedAttributes([In] int dwEventCategory, [In] int dwCount, [In, MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.U4, SizeParamIndex=1)] int[] dwAttributeIDs);
        [PreserveSig]
        int GetReturnedAttributes([In] int dwEventCategory, out int pdwCount, out IntPtr ppdwAttributeIDs);
        [PreserveSig]
        int Refresh([In] int dwConnection);
        [PreserveSig]
        int CancelRefresh([In] int dwConnection);
        [PreserveSig]
        int GetState([MarshalAs(UnmanagedType.Bool)] out bool pbActive, out int pdwBufferTime, out int pdwMaxSize, out int phClientSubscription);
        [PreserveSig]
        int SetState([In, MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.U4, SizeParamIndex=0, SizeConst=1)] int[] pbActive, [In, MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.U4, SizeParamIndex=0, SizeConst=1)] int[] pdwBufferTime, [In, MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.U4, SizeParamIndex=0, SizeConst=1)] int[] pdwMaxSize, [In] int hClientSubscription, out int pdwRevisedBufferTime, out int pdwRevisedMaxSize);
    }
}

