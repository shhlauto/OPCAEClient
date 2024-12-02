using System;
using System.Runtime.InteropServices;
namespace OPCAE.OPC.Common
{
    public class ComApi
    {
        private const uint LEVEL_SERVER_INFO_100 = 100;
        private const uint LEVEL_SERVER_INFO_101 = 0x65;
        private const int MAX_PREFERRED_LENGTH = -1;
        private const uint SV_TYPE_WORKSTATION = 1;
        private const uint SV_TYPE_SERVER = 2;
        private const int MAX_COMPUTERNAME_LENGTH = 0x1f;
        private const int RPC_C_AUTHZ_NONE = 0;
        private const int RPC_C_AUTHZ_NAME = 1;
        private const int RPC_C_AUTHZ_DCE = 2;
        private const int RPC_C_AUTHZ_DEFAULT = -1;
        private const uint CLSCTX_INPROC_SERVER = 1;
        private const uint CLSCTX_INPROC_HANDLER = 2;
        private const uint CLSCTX_LOCAL_SERVER = 4;
        private const uint CLSCTX_REMOTE_SERVER = 0x10;
        private const uint SEC_WINNT_AUTH_IDENTITY_ANSI = 1;
        private const uint SEC_WINNT_AUTH_IDENTITY_UNICODE = 2;
        private const uint URPC_C_AUTHN_DEFAULT = uint.MaxValue;
        private const uint URPC_C_AUTHZ_DEFAULT = uint.MaxValue;
        private const uint RPC_C_AUTHN_LEVEL_PKT_PRIVACY = 6;
        private const uint RPC_C_IMP_LEVEL_DEFAULT = 0;
        private const uint COLE_DEFAULT_AUTHINFO = uint.MaxValue;
        private const uint COLE_DEFAULT_PRINCIPAL = 0;
        private const uint EOAC_DEFAULT = 0x800;
        private static readonly Guid IID_IUnknown = new Guid("00000000-0000-0000-C000-000000000046");

        [DllImport("ole32.dll")]
        private static extern void CoCreateInstanceEx(ref Guid clsid, [MarshalAs(UnmanagedType.IUnknown)] object punkOuter, uint dwClsCtx, [In] ref COSERVERINFO pServerInfo, uint dwCount, [In, Out] MULTI_QI[] pResults);
        [DllImport("ole32.dll")]
        private static extern int CoInitializeSecurity(IntPtr pSecDesc, int cAuthSvc, SOLE_AUTHENTICATION_SERVICE[] asAuthSvc, IntPtr pReserved1, uint dwAuthnLevel, uint dwImpLevel, IntPtr pAuthList, uint dwCapabilities, IntPtr pReserved3);
        [DllImport("OLE32.DLL", CharSet=CharSet.Auto)]
        private static extern uint CoSetProxyBlanket(object pProxy, uint dwAuthnSvc, uint dwAuthzSvc, uint pServerPrincName, uint dwAuthnLevel, uint dwImpLevel, uint pAuthInfo, uint dwCapababilities);
        public static object CreateInstance(Guid clsid, Host host)
        {
            string str = (host != null) ? host.HostName : null;
            string str2 = (host != null) ? host.UserName : null;
            string str3 = (host != null) ? host.Password : null;
            string str4 = (host != null) ? host.Domain : null;
            GCHandle handle = GCHandle.Alloc(str2, GCHandleType.Pinned);
            GCHandle handle2 = GCHandle.Alloc(str3, GCHandleType.Pinned);
            GCHandle handle3 = GCHandle.Alloc(str4, GCHandleType.Pinned);
            GCHandle handle4 = new GCHandle();
            if ((str2 != null) && (str2 != string.Empty))
            {
                COAUTHIDENTITY coauthidentity = new COAUTHIDENTITY {
                    User = handle.AddrOfPinnedObject(),
                    UserLength = (str2 != null) ? ((uint) str2.Length) : 0,
                    Password = handle2.AddrOfPinnedObject(),
                    PasswordLength = (str3 != null) ? ((uint) str3.Length) : 0,
                    Domain = handle3.AddrOfPinnedObject(),
                    DomainLength = (str4 != null) ? ((uint) str4.Length) : 0,
                    Flags = 2
                };
                handle4 = GCHandle.Alloc(coauthidentity, GCHandleType.Pinned);
            }
            COAUTHINFO coauthinfo = new COAUTHINFO {
                dwAuthnSvc = 10,
                dwAuthzSvc = 0,
                pwszServerPrincName = IntPtr.Zero,
                dwAuthnLevel = 2,
                dwImpersonationLevel = 3,
                pAuthIdentityData = handle4.IsAllocated ? handle4.AddrOfPinnedObject() : IntPtr.Zero,
                dwCapabilities = 0
            };
            GCHandle handle5 = GCHandle.Alloc(coauthinfo, GCHandleType.Pinned);
            COSERVERINFO pServerInfo = new COSERVERINFO {
                pwszName = str,
                pAuthInfo = handle5.AddrOfPinnedObject(),
                dwReserved1 = 0,
                dwReserved2 = 0
            };
            GCHandle handle6 = GCHandle.Alloc(IID_IUnknown, GCHandleType.Pinned);
            MULTI_QI[] pResults = new MULTI_QI[1];
            pResults[0].iid = handle6.AddrOfPinnedObject();
            pResults[0].pItf = null;
            pResults[0].hr = 0;
            try
            {
                CoCreateInstanceEx(ref clsid, null, 20, ref pServerInfo, 1, pResults);
            }
            catch (Exception exception)
            {
                if (handle.IsAllocated)
                {
                    handle.Free();
                }
                if (handle2.IsAllocated)
                {
                    handle2.Free();
                }
                if (handle3.IsAllocated)
                {
                    handle3.Free();
                }
                if (handle4.IsAllocated)
                {
                    handle4.Free();
                }
                if (handle5.IsAllocated)
                {
                    handle5.Free();
                }
                if (handle6.IsAllocated)
                {
                    handle6.Free();
                }
                throw exception;
            }
            if (handle.IsAllocated)
            {
                handle.Free();
            }
            if (handle2.IsAllocated)
            {
                handle2.Free();
            }
            if (handle3.IsAllocated)
            {
                handle3.Free();
            }
            if (handle4.IsAllocated)
            {
                handle4.Free();
            }
            if (handle5.IsAllocated)
            {
                handle5.Free();
            }
            if (handle6.IsAllocated)
            {
                handle6.Free();
            }
            if (pResults[0].hr != 0)
            {
                return null;
                //throw new ApplicationException(string.Format("Create Instance Failed: 0x{0,0:X}", pResults[0].hr));
            }
            return pResults[0].pItf;
        }

        public static string[] EnumComputers()
        {
            int entriesread = 0;
            int totalentries = 0;
            IntPtr ptr;
            int num3 = NetServerEnum(IntPtr.Zero, 100, out ptr, -1, out entriesread, out totalentries, 3, IntPtr.Zero, IntPtr.Zero);
            if (num3 != 0)
            {
                throw new ApplicationException("NetApi Error = " + string.Format("0x{0,0:X}", num3));
            }
            string[] strArray = new string[entriesread];
            IntPtr ptr2 = ptr;
            for (int i = 0; i < entriesread; i++)
            {
                SERVER_INFO_100 server_info_ = (SERVER_INFO_100) Marshal.PtrToStructure(ptr2, typeof(SERVER_INFO_100));
                strArray[i] = server_info_.sv100_name;
                ptr2 = (IntPtr) (ptr2.ToInt64() + Marshal.SizeOf(typeof(SERVER_INFO_100)));
            }
            NetApiBufferFree(ptr);
            return strArray;
        }

        public static string GetComputerName()
        {
            string str = null;
            int lpnSize = 0x20;
            IntPtr lpBuffer = Marshal.AllocCoTaskMem(0x40);
            if (GetComputerNameW(lpBuffer, ref lpnSize) != 0)
            {
                str = Marshal.PtrToStringUni(lpBuffer, lpnSize);
            }
            Marshal.FreeCoTaskMem(lpBuffer);
            return str;
        }

        [DllImport("Kernel32.dll")]
        private static extern int GetComputerNameW(IntPtr lpBuffer, ref int lpnSize);
        public static void InitializeSecurity()
        {
            int num = CoInitializeSecurity(IntPtr.Zero, -1, null, IntPtr.Zero, 1, 2, IntPtr.Zero, 0, IntPtr.Zero);
            if (num != 0)
            {
                throw new ApplicationException("COM Security Error = " + string.Format("0x{0,0:X}", num));
            }
        }

        public static void InitializeSecurity(int authLevel, int impLevel, int eoac)
        {
            int num = CoInitializeSecurity(IntPtr.Zero, -1, null, IntPtr.Zero, (uint) authLevel, (uint) impLevel, IntPtr.Zero, (uint) eoac, IntPtr.Zero);
            if (num != 0)
            {
                throw new ApplicationException("COM Security Error = " + string.Format("0x{0,0:X}", num));
            }
        }

        [DllImport("Netapi32.dll")]
        public static extern int NetApiBufferFree(IntPtr buffer);
        [DllImport("Netapi32.dll")]
        private static extern int NetServerEnum(IntPtr servername, uint level, out IntPtr bufptr, int prefmaxlen, out int entriesread, out int totalentries, uint servertype, IntPtr domain, IntPtr resume_handle);
        public static void SetProxyBlanket(object srvUnk)
        {
            try
            {
                CoSetProxyBlanket(srvUnk, uint.MaxValue, uint.MaxValue, uint.MaxValue, 6, 0, 0, 0x800);
            }
            catch (Exception exception)
            {
                string message = exception.Message;
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        private struct COAUTHIDENTITY
        {
            public IntPtr User;
            public uint UserLength;
            public IntPtr Domain;
            public uint DomainLength;
            public IntPtr Password;
            public uint PasswordLength;
            public uint Flags;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        private struct COAUTHINFO
        {
            public uint dwAuthnSvc;
            public uint dwAuthzSvc;
            public IntPtr pwszServerPrincName;
            public uint dwAuthnLevel;
            public uint dwImpersonationLevel;
            public IntPtr pAuthIdentityData;
            public uint dwCapabilities;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        private struct COSERVERINFO
        {
            public uint dwReserved1;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pwszName;
            public IntPtr pAuthInfo;
            public uint dwReserved2;
        }

        public enum EOAC
        {
            NONE = 0,
            MUTUAL_AUTH = 1,
            CLOAKING = 0x10,
            STATIC_CLOAKING = 0x20,
            DYNAMIC_CLOAKING = 0x40,
            SECURE_REFS = 2,
            ACCESS_CONTROL = 4,
            APPID = 8
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        private struct MULTI_QI
        {
            public IntPtr iid;
            [MarshalAs(UnmanagedType.IUnknown)]
            public object pItf;
            public uint hr;
        }

        public enum RPC_C_AUTHN_LEVEL
        {
            DEFAULT,
            NONE,
            CONNECT,
            CALL,
            PKT,
            PKT_INTEGRITY,
            PKT_PRIVACY
        }

        public enum RPC_C_AUTHN_SVC
        {
            NONE = 0,
            DCE_public = 1,
            DCE_PUBLIC = 2,
            DEC_PUBLIC = 4,
            GSS_NEGOTIATE = 9,
            WINNT = 10,
            GSS_SCHANNEL = 14,
            GSS_KERBEROS = 0x10,
            DPA = 0x11,
            MSN = 0x12,
            DIGEST = 0x15,
            MQ = 100,
            DEFAULT = -1
        }

        public enum RPC_C_IMP_LEVEL
        {
            ANONYMOUS = 1,
            IDENTIFY = 2,
            IMPERSONATE = 3,
            DELEGATE = 4
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        private struct SERVER_INFO_100
        {
            public uint sv100_platform_id;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string sv100_name;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        private struct SOLE_AUTHENTICATION_SERVICE
        {
            public uint dwAuthnSvc;
            public uint dwAuthzSvc;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pPrincipalName;
            public int hr;
        }
    }
}

