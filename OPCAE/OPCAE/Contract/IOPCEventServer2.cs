using OPCAE;
using System;
using System.Runtime.InteropServices;
namespace OPCAE.OPCAE.Contract
{

    [ComImport, Guid("71BBE88E-9564-4bcd-BCFC-71C558D94F2D"), ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IOPCEventServer2
    {
        [PreserveSig]
        int GetStatus(out IntPtr ppEventServerStatus);
        [PreserveSig]
        int CreateEventSubscription([In, MarshalAs(UnmanagedType.Bool)] bool bActive, [In] int dwBufferTime, [In] int dwMaxSize, [In] int hClientSubscription, [In] ref Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out object ppUnk, out int pdwRevisedBufferTime, out int pdwRevisedMaxSize);
        [PreserveSig]
        int QueryAvailableFilters(out OPCAEFilters pdwFilterMask);
        [PreserveSig]
        int QueryEventCategories([In] int dwEventType, out int pdwCount, out IntPtr ppdwEventCategories, out IntPtr ppszEventCategoryDescs);
        [PreserveSig]
        int QueryConditionNames([In] int dwEventCategory, out int pdwCount, out IntPtr ppszConditionNames);
        [PreserveSig]
        int QuerySubConditionNames([In, MarshalAs(UnmanagedType.LPWStr)] string szConditionName, out int pdwCount, out IntPtr ppszSubConditionNames);
        [PreserveSig]
        int QuerySourceConditions([In, MarshalAs(UnmanagedType.LPWStr)] string szSource, out int pdwCount, out IntPtr ppszConditionNames);
        [PreserveSig]
        int QueryEventAttributes([In] int dwEventCategory, out int pdwCount, out IntPtr ppdwAttrIDs, out IntPtr ppszAttrDescs, out IntPtr ppvtAttrTypes);
        [PreserveSig]
        int TranslateToItemIDs([In, MarshalAs(UnmanagedType.LPWStr)] string szSource, [In] int dwEventCategory, [In, MarshalAs(UnmanagedType.LPWStr)] string szConditionName, [In, MarshalAs(UnmanagedType.LPWStr)] string szSubconditionName, [In] int dwCount, [In, MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.U4, SizeParamIndex=4)] int[] pdwAssocAttrIDs, out IntPtr ppszAttrItemIDs, out IntPtr ppszNodeNames, out IntPtr ppCLSIDs);
        [PreserveSig]
        int GetConditionState([In, MarshalAs(UnmanagedType.LPWStr)] string szSource, [In, MarshalAs(UnmanagedType.LPWStr)] string szConditionName, [In] int dwNumEventAttrs, [In, MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.U4, SizeParamIndex=2)] int[] pdwAttributeIDs, out IntPtr ppConditionState);
        [PreserveSig]
        int EnableConditionByArea([In] int dwNumAreas, [In, MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.LPWStr, SizeParamIndex=0)] string[] pszAreas);
        [PreserveSig]
        int EnableConditionBySource([In] int dwNumSources, [In, MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.LPWStr, SizeParamIndex=0)] string[] pszSources);
        [PreserveSig]
        int DisableConditionByArea([In] int dwNumAreas, [In, MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.LPWStr, SizeParamIndex=0)] string[] pszAreas);
        [PreserveSig]
        int DisableConditionBySource([In] int dwNumSources, [In, MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.LPWStr, SizeParamIndex=0)] string[] pszSources);
        [PreserveSig]
        int AckCondition([In] int dwCount, [In, MarshalAs(UnmanagedType.LPWStr)] string szAcknowledgerID, [In, MarshalAs(UnmanagedType.LPWStr)] string szComment, [In, MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.LPWStr, SizeParamIndex=0)] string[] pszSource, [In, MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.LPWStr, SizeParamIndex=0)] string[] pszConditionName, [In, MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.U8, SizeParamIndex=0)] FILETIME[] pftActiveTime, [In, MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.U4, SizeParamIndex=0)] int[] pdwCookie, out IntPtr ppErrors);
        [PreserveSig]
        int CreateAreaBrowser([In] ref Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out object ppUnk);
        [PreserveSig]
        int EnableConditionByArea2([In] int dwNumAreas, [In, MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.LPWStr, SizeParamIndex=0)] string[] pszAreas, out IntPtr ppErrors);
        [PreserveSig]
        int EnableConditionBySource2([In] int dwNumSources, [In, MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.LPWStr, SizeParamIndex=0)] string[] pszSources, out IntPtr ppErrors);
        [PreserveSig]
        int DisableConditionByArea2([In] int dwNumAreas, [In, MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.LPWStr, SizeParamIndex=0)] string[] pszAreas, out IntPtr ppErrors);
        [PreserveSig]
        int DisableConditionBySource2([In] int dwNumSources, [In, MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.LPWStr, SizeParamIndex=0)] string[] pszSources, out IntPtr ppErrors);
        [PreserveSig]
        int GetEnableStateByArea([In] int dwNumAreas, [In, MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.LPWStr, SizeParamIndex=0)] string[] pszAreas, out IntPtr pbEnabled, out IntPtr pbEffectivelyEnabled, out IntPtr ppErrors);
        [PreserveSig]
        int GetEnableStateBySource([In] int dwNumSources, [In, MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.LPWStr, SizeParamIndex=0)] string[] pszSources, out IntPtr pbEnabled, out IntPtr pbEffectivelyEnabled, out IntPtr ppErrors);
    }
}

