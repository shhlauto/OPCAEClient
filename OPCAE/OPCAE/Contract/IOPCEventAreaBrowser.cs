namespace OPCAE.OPCAE.Contract
{
    using OPCAE;
    using System;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), ComVisible(true), Guid("65168857-5783-11D1-84A0-00608CB8A7E9")]
    public interface IOPCEventAreaBrowser
    {
        [PreserveSig]
        int ChangeBrowsePosition([In] OPCAEBrowseDirection dwBrowseDirection, [In, MarshalAs(UnmanagedType.LPWStr)] string szString);
        [PreserveSig]
        int BrowseOPCAreas([In] OPCAEBrowseType dwBrowseFilterType, [In, MarshalAs(UnmanagedType.LPWStr)] string szFilterCriteria, [MarshalAs(UnmanagedType.IUnknown)] out object ppIEnumString);
        [PreserveSig]
        int GetQualifiedAreaName([In, MarshalAs(UnmanagedType.LPWStr)] string szAreaName, [MarshalAs(UnmanagedType.LPWStr)] out string pszQualifiedAreaName);
        [PreserveSig]
        int GetQualifiedSourceName([In, MarshalAs(UnmanagedType.LPWStr)] string szSourceName, [MarshalAs(UnmanagedType.LPWStr)] out string pszQualifiedSourceName);
    }
}

