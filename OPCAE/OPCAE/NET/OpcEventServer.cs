using Microsoft.Win32;
using OPCAE.OPC;
using OPCAE.OPC.Common;
using OPCAE.OPCAE.Contract;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Text;
namespace OPCAE.OPCAE.NET
{
    [SuppressUnmanagedCodeSecurity, ComVisible(true), ReflectionPermission(SecurityAction.Assert, Unrestricted=true), SecurityPermission(SecurityAction.Assert, UnmanagedCode=true)]
    public class OpcEventServer : IOPCShutdown
    {
        public static bool myErrorsAsExecptions;
        private string myServerName;
        private Host myHostInfo;
        private object OPCeventServerObj;
        public IOPCEventServer ifAEServer;
        private IOPCEventServer2 ifAEServer2;
        private IOPCCommon ifCommon;
        private IOPCSecurityNT ifSecurityNT;
        private IOPCSecurityPrivate ifSecurityPriv;
        private UCOMIConnectionPointContainer evCpointcontainer;
        private UCOMIConnectionPoint shutdowncpoint;
        private int shutdowncookie;
        private bool myConnectThroughNIOS;

        public event ShutdownRequestEventHandler ShutdownRequested;

        public OpcEventServer()
        {
            this.shutdowncpoint = null;
            this.shutdowncookie = 0;
            this.myConnectThroughNIOS = false;
            //this.create(true);
        }

        public OpcEventServer(bool appRegister)
        {
            this.shutdowncpoint = null;
            this.shutdowncookie = 0;
            this.myConnectThroughNIOS = false;
            this.create(appRegister);
        }

        public int AckCondition(string AcknowledgerID, string Comment, string[] Source, string[] ConditionName, FILETIME[] ActiveTime, int[] Cookie, out int[] Errors)
        {
            Errors = null;
            IntPtr ptr;
            int hresultcode = this.ifAEServer.AckCondition(Source.Length, AcknowledgerID, Comment, Source, ConditionName, ActiveTime, Cookie, out ptr);
            if (HRESULTS.Failed(hresultcode))
            {
                if (myErrorsAsExecptions)
                {
                    throw new OPCException(hresultcode, ErrorDescriptions.GetErrorDescription(hresultcode));
                }
                return hresultcode;
            }
            long num2 = (long) ptr;
            if (num2 == 0L)
            {
                if (myErrorsAsExecptions)
                {
                    throw new OPCException(-2147467259, ErrorDescriptions.GetErrorDescription(-2147467259));
                }
                return -2147467259;
            }
            Errors = new int[Source.Length];
            for (int i = 0; i < Source.Length; i++)
            {
                Errors[i] = Marshal.ReadInt32((IntPtr) num2);
                num2 += 4L;
            }
            Marshal.FreeCoTaskMem(ptr);
            return hresultcode;
        }

        private void AdviseIOPCShutdown()
        {
            Type type = typeof(IOPCShutdown);
            Guid gUID = type.GUID;
            this.evCpointcontainer.FindConnectionPoint(ref gUID, out this.shutdowncpoint);
            if (this.shutdowncpoint != null)
            {
                this.shutdowncpoint.Advise(this, out this.shutdowncookie);
            }
        }

        public int ChangeUser()
        {
            if (this.ifSecurityNT == null)
            {
                if (myErrorsAsExecptions)
                {
                    throw new OPCException(-2147467263, ErrorDescriptions.GetErrorDescription(-2147467263));
                }
                return -2147467263;
            }
            int hresultcode = this.ifSecurityNT.ChangeUser();
            if (myErrorsAsExecptions && HRESULTS.Failed(hresultcode))
            {
                throw new OPCException(hresultcode, ErrorDescriptions.GetErrorDescription(hresultcode));
            }
            return hresultcode;
        }

        public int Connect(Guid ClsidOPCserver)
        {
            this.Disconnect();
            Type typeFromCLSID = Type.GetTypeFromCLSID(ClsidOPCserver);
            if (typeFromCLSID == null)
            {
                if (myErrorsAsExecptions)
                {
                    throw new OPCException(-1073479663, ErrorDescriptions.GetErrorDescription(-1073479663));
                }
                return -1073479663;
            }
            int hresultcode = this.SetInterfaces(Activator.CreateInstance(typeFromCLSID));
            if (HRESULTS.Succeeded(hresultcode))
            {
                this.myServerName = ClsidOPCserver.ToString();
            }
            if (myErrorsAsExecptions && HRESULTS.Failed(hresultcode))
            {
                throw new OPCException(hresultcode, ErrorDescriptions.GetErrorDescription(hresultcode));
            }
            return hresultcode;
        }

        public int Connect(string SrvName)
        {
            int num;
            if (SrvName.StartsWith(@"\\"))
            {
                int index = SrvName.IndexOf('\\', 2);
                string srvName = SrvName.Substring(index + 1);
                string computerName = SrvName.Substring(2, index - 2);
                if (srvName == "")
                {
                    return -2147024809;
                }
                if (computerName == "")
                {
                    num = this.ConnectLocal(SrvName);
                }
                else if (this.myConnectThroughNIOS)
                {
                    num = this.ConnectRemoteNIOS(computerName, srvName);
                }
                else
                {
                    Host accessInfo = new Host(computerName);
                    num = this.Connect(accessInfo, srvName);
                }
            }
            else
            {
                num = this.ConnectLocal(SrvName);
            }
            if (myErrorsAsExecptions && HRESULTS.Failed(num))
            {
                throw new OPCException(num, ErrorDescriptions.GetErrorDescription(num));
            }
            return num;
        }

        public int Connect(Host accessInfo, Guid ClsidOPCserver)
        {
            this.Disconnect();
            int hresultcode = this.SetInterfaces(ComApi.CreateInstance(ClsidOPCserver, accessInfo));
            if (HRESULTS.Succeeded(hresultcode))
            {
                this.myHostInfo = accessInfo;
                this.myServerName = ClsidOPCserver.ToString();
            }
            if (myErrorsAsExecptions && HRESULTS.Failed(hresultcode))
            {
                throw new OPCException(hresultcode, ErrorDescriptions.GetErrorDescription(hresultcode));
            }
            return hresultcode;
        }

        public int Connect(Host accessInfo, string SrvName)
        {
            if (this.myConnectThroughNIOS)
            {
                int num = this.ConnectRemoteNIOS(accessInfo.HostName, SrvName);
                if (myErrorsAsExecptions && HRESULTS.Failed(num))
                {
                    throw new OPCException(num, ErrorDescriptions.GetErrorDescription(num));
                }
                return num;
            }
            OpcAEServerBrowser browser = new OpcAEServerBrowser(accessInfo.HostName);
            Guid empty = Guid.Empty;
            bool flag = false;
            string[] strArray;
            Guid[] guidArray;
            browser.GetServerList(out strArray, out guidArray);
            if (strArray == null)
            {
                if (myErrorsAsExecptions)
                {
                    throw new OPCException(-1073479663, ErrorDescriptions.GetErrorDescription(-1073479663));
                }
                return -1073479663;
            }
            for (int i = 0; i < strArray.Length; i++)
            {
                if (strArray[i] == SrvName)
                {
                    empty = guidArray[i];
                    flag = true;
                    break;
                }
            }
            if (!flag)
            {
                if (myErrorsAsExecptions)
                {
                    throw new OPCException(-1073479663, ErrorDescriptions.GetErrorDescription(-1073479663));
                }
                return -1073479663;
            }
            strArray = null;
            guidArray = null;
            int hresultcode = this.SetInterfaces(ComApi.CreateInstance(empty, accessInfo));
            if (HRESULTS.Succeeded(hresultcode))
            {
                this.myHostInfo = accessInfo;
                this.myServerName = SrvName;
            }
            if (myErrorsAsExecptions && HRESULTS.Failed(hresultcode))
            {
                throw new OPCException(hresultcode, ErrorDescriptions.GetErrorDescription(hresultcode));
            }
            return hresultcode;
        }

        public int Connect(string ComputerName, Guid ClsidOPCserver)
        {
            this.Disconnect();
            Host host = new Host(ComputerName);
            int hresultcode = this.SetInterfaces(ComApi.CreateInstance(ClsidOPCserver, host));
            if (HRESULTS.Succeeded(hresultcode))
            {
                this.myHostInfo.HostName = ComputerName;
                this.myServerName = ClsidOPCserver.ToString();
            }
            if (myErrorsAsExecptions && HRESULTS.Failed(hresultcode))
            {
                throw new OPCException(hresultcode, ErrorDescriptions.GetErrorDescription(hresultcode));
            }
            return hresultcode;
        }

        public int Connect(string ComputerName, string SrvName)
        {
            int num;
            if ((ComputerName != null) && (ComputerName != ""))
            {
                if (this.myConnectThroughNIOS)
                {
                    num = this.ConnectRemoteNIOS(ComputerName, SrvName);
                }
                else
                {
                    Host accessInfo = new Host(ComputerName);
                    num = this.Connect(accessInfo, SrvName);
                }
            }
            else
            {
                num = this.Connect(SrvName);
            }
            if (myErrorsAsExecptions && HRESULTS.Failed(num))
            {
                throw new OPCException(num, ErrorDescriptions.GetErrorDescription(num));
            }
            return num;
        }

        private int ConnectLocal(string SrvName)
        {
            Type typeFromProgID = null;
            this.Disconnect();
            typeFromProgID = Type.GetTypeFromProgID(SrvName, false);
            if (typeFromProgID == null)
            {
                return -1073479663;
            }
            int hresultcode = this.SetInterfaces(Activator.CreateInstance(typeFromProgID));
            if (HRESULTS.Succeeded(hresultcode))
            {
                this.myHostInfo.HostName = ComApi.GetComputerName();
            }
            this.myServerName = SrvName;
            return hresultcode;
        }

        private int ConnectRemoteNIOS(string ComputerName, string SrvName)
        {
            Type type = null;
            this.Disconnect();
            type = Type.GetTypeFromProgID(SrvName, ComputerName, false);
            if (type == null)
            {
                return -1073479663;
            }
            int hresultcode = this.SetInterfaces(Activator.CreateInstance(type));
            if (HRESULTS.Succeeded(hresultcode))
            {
                this.myHostInfo.HostName = ComputerName;
            }
            this.myServerName = SrvName;
            return hresultcode;
        }

        private void create(bool appRegister)
        {
            myErrorsAsExecptions = false;
            this.myHostInfo = new Host();
            if (appRegister)
            {
                this.RegisterAppCallbackRights();
            }
        }

        public int CreateAreaBrowser(out AreaBrowser browser)
        {
            browser = new AreaBrowser();
            int hresultcode = browser.Create(this);
            if (HRESULTS.Failed(hresultcode))
            {
                browser.Dispose();
                browser = null;
            }
            if (myErrorsAsExecptions && HRESULTS.Failed(hresultcode))
            {
                throw new OPCException(hresultcode, ErrorDescriptions.GetErrorDescription(hresultcode));
            }
            return hresultcode;
        }

        public int CreateEventSubscription(bool Active, int BufferTime, int MaxSize, int ClientSubscription, ref Guid riid, out object ppUnk, out int RevisedBufferTime, out int RevisedMaxSize) => 
            this.ifAEServer.CreateEventSubscription(Active, BufferTime, MaxSize, ClientSubscription, ref riid, out ppUnk, out RevisedBufferTime, out RevisedMaxSize);

        public int DisableConditionByArea(string[] Areas)
        {
            int hresultcode = this.ifAEServer.DisableConditionByArea(Areas.Length, Areas);
            if (myErrorsAsExecptions && HRESULTS.Failed(hresultcode))
            {
                throw new OPCException(hresultcode, ErrorDescriptions.GetErrorDescription(hresultcode));
            }
            return hresultcode;
        }

        public int DisableConditionByArea2(string[] Areas, out int[] Errors)
        {
            Errors = null;
            if (this.ifAEServer2 == null)
            {
                if (myErrorsAsExecptions)
                {
                    throw new OPCException(-2147467263, ErrorDescriptions.GetErrorDescription(-2147467263));
                }
                return -2147467263;
            }
            IntPtr ptr;
            int hresultcode = this.ifAEServer2.DisableConditionByArea2(Areas.Length, Areas, out ptr);
            if (HRESULTS.Failed(hresultcode))
            {
                if (myErrorsAsExecptions)
                {
                    throw new OPCException(hresultcode, ErrorDescriptions.GetErrorDescription(hresultcode));
                }
                return hresultcode;
            }
            long num2 = (long) ptr;
            if (num2 == 0L)
            {
                if (myErrorsAsExecptions)
                {
                    throw new OPCException(-2147467259, ErrorDescriptions.GetErrorDescription(-2147467259));
                }
                return -2147467259;
            }
            Errors = new int[Areas.Length];
            for (int i = 0; i < Areas.Length; i++)
            {
                Errors[i] = Marshal.ReadInt32((IntPtr) num2);
                num2 += 4L;
            }
            Marshal.FreeCoTaskMem(ptr);
            return hresultcode;
        }

        public int DisableConditionBySource(string[] Sources)
        {
            int hresultcode = this.ifAEServer.DisableConditionBySource(Sources.Length, Sources);
            if (myErrorsAsExecptions && HRESULTS.Failed(hresultcode))
            {
                throw new OPCException(hresultcode, ErrorDescriptions.GetErrorDescription(hresultcode));
            }
            return hresultcode;
        }

        public int DisableConditionBySource2(string[] Sources, out int[] Errors)
        {
            Errors = null;
            if (this.ifAEServer2 == null)
            {
                if (myErrorsAsExecptions)
                {
                    throw new OPCException(-2147467263, ErrorDescriptions.GetErrorDescription(-2147467263));
                }
                return -2147467263;
            }
            IntPtr ptr;
            int hresultcode = this.ifAEServer2.DisableConditionBySource2(Sources.Length, Sources, out ptr);
            if (HRESULTS.Failed(hresultcode))
            {
                if (myErrorsAsExecptions)
                {
                    throw new OPCException(hresultcode, ErrorDescriptions.GetErrorDescription(hresultcode));
                }
                return hresultcode;
            }
            long num2 = (long) ptr;
            if (num2 == 0L)
            {
                if (myErrorsAsExecptions)
                {
                    throw new OPCException(-2147467259, ErrorDescriptions.GetErrorDescription(-2147467259));
                }
                return -2147467259;
            }
            Errors = new int[Sources.Length];
            for (int i = 0; i < Sources.Length; i++)
            {
                Errors[i] = Marshal.ReadInt32((IntPtr) num2);
                num2 += 4L;
            }
            Marshal.FreeCoTaskMem(ptr);
            return hresultcode;
        }

        public void Disconnect()
        {
            if (this.shutdowncpoint != null)
            {
                if (this.shutdowncookie != 0)
                {
                    try
                    {
                        this.shutdowncpoint.Unadvise(this.shutdowncookie);
                    }
                    catch
                    {
                    }
                    this.shutdowncookie = 0;
                }
                try
                {
                    Marshal.ReleaseComObject(this.shutdowncpoint);
                }
                catch
                {
                }
                this.shutdowncpoint = null;
            }
            Register.Disconnect(this);
            this.evCpointcontainer = null;
            this.ifAEServer = null;
            this.ifAEServer2 = null;
            this.ifCommon = null;
            this.ifSecurityNT = null;
            this.ifSecurityPriv = null;
            if (this.OPCeventServerObj != null)
            {
                try
                {
                    Marshal.ReleaseComObject(this.OPCeventServerObj);
                }
                catch
                {
                }
                this.OPCeventServerObj = null;
            }
        }

        public int EnableConditionByArea(string[] Areas)
        {
            int hresultcode = this.ifAEServer.EnableConditionByArea(Areas.Length, Areas);
            if (myErrorsAsExecptions && HRESULTS.Failed(hresultcode))
            {
                throw new OPCException(hresultcode, ErrorDescriptions.GetErrorDescription(hresultcode));
            }
            return hresultcode;
        }

        public int EnableConditionByArea2(string[] Areas, out int[] Errors)
        {
            Errors = null;
            if (this.ifAEServer2 == null)
            {
                if (myErrorsAsExecptions)
                {
                    throw new OPCException(-2147467263, ErrorDescriptions.GetErrorDescription(-2147467263));
                }
                return -2147467263;
            }
            IntPtr ptr;
            int hresultcode = this.ifAEServer2.EnableConditionByArea2(Areas.Length, Areas, out ptr);
            if (HRESULTS.Failed(hresultcode))
            {
                if (myErrorsAsExecptions)
                {
                    throw new OPCException(hresultcode, ErrorDescriptions.GetErrorDescription(hresultcode));
                }
                return hresultcode;
            }
            long num2 = (long) ptr;
            if (num2 == 0L)
            {
                if (myErrorsAsExecptions)
                {
                    throw new OPCException(-2147467259, ErrorDescriptions.GetErrorDescription(-2147467259));
                }
                return -2147467259;
            }
            Errors = new int[Areas.Length];
            for (int i = 0; i < Areas.Length; i++)
            {
                Errors[i] = Marshal.ReadInt32((IntPtr) num2);
                num2 += 4L;
            }
            Marshal.FreeCoTaskMem(ptr);
            return hresultcode;
        }

        public int EnableConditionBySource(string[] Sources)
        {
            int hresultcode = this.ifAEServer.EnableConditionBySource(Sources.Length, Sources);
            if (myErrorsAsExecptions && HRESULTS.Failed(hresultcode))
            {
                throw new OPCException(hresultcode, ErrorDescriptions.GetErrorDescription(hresultcode));
            }
            return hresultcode;
        }

        public int EnableConditionBySource2(string[] Sources, out int[] Errors)
        {
            Errors = null;
            if (this.ifAEServer2 == null)
            {
                if (myErrorsAsExecptions)
                {
                    throw new OPCException(-2147467263, ErrorDescriptions.GetErrorDescription(-2147467263));
                }
                return -2147467263;
            }
            IntPtr ptr;
            int hresultcode = this.ifAEServer2.EnableConditionBySource2(Sources.Length, Sources, out ptr);
            if (HRESULTS.Failed(hresultcode))
            {
                if (myErrorsAsExecptions)
                {
                    throw new OPCException(hresultcode, ErrorDescriptions.GetErrorDescription(hresultcode));
                }
                return hresultcode;
            }
            long num2 = (long) ptr;
            if (num2 == 0L)
            {
                if (myErrorsAsExecptions)
                {
                    throw new OPCException(-2147467259, ErrorDescriptions.GetErrorDescription(-2147467259));
                }
                return -2147467259;
            }
            Errors = new int[Sources.Length];
            for (int i = 0; i < Sources.Length; i++)
            {
                Errors[i] = Marshal.ReadInt32((IntPtr) num2);
                num2 += 4L;
            }
            Marshal.FreeCoTaskMem(ptr);
            return hresultcode;
        }

        ~OpcEventServer()
        {
            this.Disconnect();
        }

        public RegistryKey FindAppExe(RegistryKey appid, string name)
        {
            string[] subKeyNames;
            try
            {
                subKeyNames = appid.GetSubKeyNames();
            }
            catch
            {
                return null;
            }
            foreach (string str in subKeyNames)
            {
                if (str == name)
                {
                    try
                    {
                        return appid.OpenSubKey(str, true);
                    }
                    catch
                    {
                    }
                }
            }
            return null;
        }

        public int GetConditionState(OpcEventServer opcEventServer, List<List<string>> opcServerInfoList, string Source, string ConditionName, int[] AttributeIDs, out OPCConditionState CondState)
        {
            CondState = null;
            IntPtr ptr;
            int hresultcode = this.ifAEServer.GetConditionState(Source, ConditionName, AttributeIDs.Length, AttributeIDs, out ptr);
            if (HRESULTS.Failed(hresultcode))
            {
                if (myErrorsAsExecptions)
                {
                    throw new OPCException(hresultcode, ErrorDescriptions.GetErrorDescription(hresultcode));
                }
                return hresultcode;
            }
            CondState = new OPCConditionState();
            if (IntPtr.Size < 8)
            {
                int num2 = (int) ptr;
                CondState.State = Marshal.ReadInt16((IntPtr) num2);
                num2 += 4;
                IntPtr ptr2 = Marshal.ReadIntPtr((IntPtr) num2);
                CondState.ActiveSubCondition = Marshal.PtrToStringUni(ptr2);
                Marshal.FreeCoTaskMem(ptr2);
                num2 += 4;
                ptr2 = Marshal.ReadIntPtr((IntPtr) num2);
                CondState.ASCDefinition = Marshal.PtrToStringUni(ptr2);
                Marshal.FreeCoTaskMem(ptr2);
                num2 += 4;
                CondState.ASCSeverity = Marshal.ReadInt32((IntPtr) num2);
                num2 += 4;
                ptr2 = Marshal.ReadIntPtr((IntPtr) num2);
                CondState.ASCDescription = Marshal.PtrToStringUni(ptr2);
                for (int j = 0, l = opcServerInfoList.Count; j < l; j++)
                {
                    if (opcServerInfoList[j][0].Contains(opcEventServer.myHostInfo.HostName)&&opcServerInfoList[j][2].Contains("霍尼"))
                    {
                        Encoding ISO88591Encoding = Encoding.GetEncoding("ISO-8859-1");
                        Encoding GB2312Encoding = Encoding.GetEncoding("gb2312"); //这个地方很特殊，必须利用GB2312编码
                        byte[] srcBytes_0 = ISO88591Encoding.GetBytes(Marshal.PtrToStringUni(ptr2));
                        //将原本存储ISO-8859-1的字节数组当成GB2312转换成目标编码(关键步骤)
                        byte[] dstBytes_0 = Encoding.Convert(GB2312Encoding, Encoding.Default, srcBytes_0);
                        char[] dstChars_0 = new char[Encoding.Default.GetCharCount(dstBytes_0, 0, dstBytes_0.Length)];
                        Encoding.Default.GetChars(dstBytes_0, 0, dstBytes_0.Length, dstChars_0, 0);//利用char数组存储字符
                        string sResult = new string(dstChars_0);
                        CondState.ASCDescription = sResult;
                    }
                }
                Marshal.FreeCoTaskMem(ptr2);
                num2 += 4;
                CondState.Quality = Marshal.ReadInt16((IntPtr) num2);
                num2 += 4;
                CondState.LastAckTime = DateTime.FromFileTime(Marshal.ReadInt64((IntPtr)num2));//DateTime.FromFileTimeUtc(Marshal.ReadInt64((IntPtr) num2));
                num2 += 8;
                CondState.SubCondLastActive = DateTime.FromFileTime(Marshal.ReadInt64((IntPtr)num2));//DateTime.FromFileTimeUtc(Marshal.ReadInt64((IntPtr) num2));
                num2 += 8;
                CondState.CondLastActive = DateTime.FromFileTime(Marshal.ReadInt64((IntPtr)num2));//DateTime.FromFileTimeUtc(Marshal.ReadInt64((IntPtr) num2));
                num2 += 8;
                CondState.CondLastInactive = DateTime.FromFileTime(Marshal.ReadInt64((IntPtr)num2));//DateTime.FromFileTimeUtc(Marshal.ReadInt64((IntPtr) num2));
                num2 += 8;
                ptr2 = Marshal.ReadIntPtr((IntPtr) num2);
                CondState.AcknowledgerID = Marshal.PtrToStringUni(ptr2);
                Marshal.FreeCoTaskMem(ptr2);
                num2 += 4;
                ptr2 = Marshal.ReadIntPtr((IntPtr) num2);
                CondState.Comment = Marshal.PtrToStringUni(ptr2);
                Marshal.FreeCoTaskMem(ptr2);
                num2 += 4;
                CondState.NumSCs = Marshal.ReadInt32((IntPtr) num2);
                num2 += 4;
                CondState.SCNames = new string[CondState.NumSCs];
                IntPtr ptr3 = Marshal.ReadIntPtr((IntPtr) num2);
                int num3 = ptr3.ToInt32();
                for (int num4 = 0; num4 < CondState.NumSCs; num4++)
                {
                    ptr2 = Marshal.ReadIntPtr((IntPtr) num3);
                    CondState.SCNames[num4] = Marshal.PtrToStringUni(ptr2);
                    Marshal.FreeCoTaskMem(ptr2);
                    num3 += IntPtr.Size;
                }
                Marshal.FreeCoTaskMem(ptr3);
                num2 += IntPtr.Size;
                CondState.SCDefinitions = new string[CondState.NumSCs];
                ptr3 = Marshal.ReadIntPtr((IntPtr) num2);
                num3 = ptr3.ToInt32();
                for (int num5 = 0; num5 < CondState.NumSCs; num5++)
                {
                    ptr2 = Marshal.ReadIntPtr((IntPtr) num3);
                    CondState.SCDefinitions[num5] = Marshal.PtrToStringUni(ptr2);
                    Marshal.FreeCoTaskMem(ptr2);
                    num3 += IntPtr.Size;
                }
                Marshal.FreeCoTaskMem(ptr3);
                num2 += IntPtr.Size;
                CondState.SCSeverities = new int[CondState.NumSCs];
                ptr3 = Marshal.ReadIntPtr((IntPtr) num2);
                num3 = ptr3.ToInt32();
                for (int num6 = 0; num6 < CondState.NumSCs; num6++)
                {
                    CondState.SCSeverities[num6] = Marshal.ReadInt32((IntPtr) num3);
                    num3 += 4;
                }
                Marshal.FreeCoTaskMem(ptr3);
                num2 += IntPtr.Size;
                CondState.SCDescriptions = new string[CondState.NumSCs];
                ptr3 = Marshal.ReadIntPtr((IntPtr) num2);
                num3 = ptr3.ToInt32();
                for (int num7 = 0; num7 < CondState.NumSCs; num7++)
                {
                    ptr2 = Marshal.ReadIntPtr((IntPtr) num3);
                    CondState.SCDescriptions[num7] = Marshal.PtrToStringUni(ptr2);
                    Marshal.FreeCoTaskMem(ptr2);
                    num3 += IntPtr.Size;
                }
                Marshal.FreeCoTaskMem(ptr3);
                num2 += IntPtr.Size;
                CondState.NumEventAttrs = Marshal.ReadInt32((IntPtr) num2);
                num2 += 4;
                CondState.EventAttributes = new object[CondState.NumEventAttrs];
                ptr3 = Marshal.ReadIntPtr((IntPtr) num2);
                num3 = ptr3.ToInt32();
                for (int num8 = 0; num8 < CondState.NumEventAttrs; num8++)
                {
                    CondState.EventAttributes[num8] = Marshal.GetObjectForNativeVariant((IntPtr) num3);
                    DUMMY_VARIANT.VariantClear((IntPtr) num3);
                    num3 += DUMMY_VARIANT.ConstSize;
                }
                Marshal.FreeCoTaskMem(ptr3);
                num2 += IntPtr.Size;
                CondState.Errors = new int[CondState.NumEventAttrs];
                ptr3 = Marshal.ReadIntPtr((IntPtr) num2);
                num3 = ptr3.ToInt32();
                for (int num9 = 0; num9 < CondState.NumEventAttrs; num9++)
                {
                    CondState.Errors[num9] = Marshal.ReadInt32((IntPtr) num3);
                    num3 += 4;
                }
                Marshal.FreeCoTaskMem(ptr3);
                num2 += IntPtr.Size;
                return hresultcode;
            }
            long num10 = (long) ptr;
            CondState.State = Marshal.ReadInt16((IntPtr) num10);
            num10 += 8L;
            IntPtr ptr4 = Marshal.ReadIntPtr((IntPtr) num10);
            CondState.ActiveSubCondition = Marshal.PtrToStringUni(ptr4);
            Marshal.FreeCoTaskMem(ptr4);
            num10 += 8L;
            ptr4 = Marshal.ReadIntPtr((IntPtr) num10);
            CondState.ASCDefinition = Marshal.PtrToStringUni(ptr4);
            Marshal.FreeCoTaskMem(ptr4);
            num10 += 8L;
            CondState.ASCSeverity = Marshal.ReadInt32((IntPtr) num10);
            num10 += 8L;
            ptr4 = Marshal.ReadIntPtr((IntPtr) num10);
            CondState.ASCDescription = Marshal.PtrToStringUni(ptr4);
            for (int j = 0, l = opcServerInfoList.Count; j < l; j++)
            {
                if (opcServerInfoList[j][2].Contains("霍尼"))
                {
                    Encoding ISO88591Encoding = Encoding.GetEncoding("ISO-8859-1");
                    Encoding GB2312Encoding = Encoding.GetEncoding("gb2312"); //这个地方很特殊，必须利用GB2312编码
                    byte[] srcBytes = ISO88591Encoding.GetBytes(CondState.ASCDescription);
                    //将原本存储ISO-8859-1的字节数组当成GB2312转换成目标编码(关键步骤)
                    byte[] dstBytes = Encoding.Convert(GB2312Encoding, Encoding.Default, srcBytes);
                    char[] dstChars = new char[Encoding.Default.GetCharCount(dstBytes, 0, dstBytes.Length)];
                    Encoding.Default.GetChars(dstBytes, 0, dstBytes.Length, dstChars, 0);//利用char数组存储字符
                    string sResult = new string(dstChars);
                    CondState.ASCDescription = sResult;
                }
            }
            Marshal.FreeCoTaskMem(ptr4);
            num10 += 8L;
            CondState.Quality = Marshal.ReadInt16((IntPtr) num10);
            num10 += 8L;
            CondState.LastAckTime = DateTime.FromFileTime(Marshal.ReadInt64((IntPtr)num10));//DateTime.FromFileTimeUtc(Marshal.ReadInt64((IntPtr) num10));
            num10 += 8L;
            CondState.SubCondLastActive = DateTime.FromFileTime(Marshal.ReadInt64((IntPtr)num10)); //DateTime.FromFileTimeUtc(Marshal.ReadInt64((IntPtr) num10));
            num10 += 8L;
            CondState.CondLastActive = DateTime.FromFileTime(Marshal.ReadInt64((IntPtr)num10)); //DateTime.FromFileTimeUtc(Marshal.ReadInt64((IntPtr) num10));
            num10 += 8L;
            CondState.CondLastInactive = DateTime.FromFileTime(Marshal.ReadInt64((IntPtr) num10)); //DateTime.FromFileTimeUtc(Marshal.ReadInt64((IntPtr)num10));
            num10 += 8L;
            ptr4 = Marshal.ReadIntPtr((IntPtr) num10);
            CondState.AcknowledgerID = Marshal.PtrToStringUni(ptr4);
            Marshal.FreeCoTaskMem(ptr4);
            num10 += 8L;
            ptr4 = Marshal.ReadIntPtr((IntPtr) num10);
            CondState.Comment = Marshal.PtrToStringUni(ptr4);
            Marshal.FreeCoTaskMem(ptr4);
            num10 += 8L;
            CondState.NumSCs = Marshal.ReadInt32((IntPtr) num10);
            num10 += 8L;
            CondState.SCNames = new string[CondState.NumSCs];
            IntPtr ptr5 = Marshal.ReadIntPtr((IntPtr) num10);
            long num11 = ptr5.ToInt32();
            for (int i = 0; i < CondState.NumSCs; i++)
            {
                ptr4 = Marshal.ReadIntPtr((IntPtr) num11);
                CondState.SCNames[i] = Marshal.PtrToStringUni(ptr4);
                Marshal.FreeCoTaskMem(ptr4);
                num11 += IntPtr.Size;
            }
            Marshal.FreeCoTaskMem(ptr5);
            num10 += IntPtr.Size;
            CondState.SCDefinitions = new string[CondState.NumSCs];
            ptr5 = Marshal.ReadIntPtr((IntPtr) num10);
            num11 = ptr5.ToInt32();
            for (int j = 0; j < CondState.NumSCs; j++)
            {
                ptr4 = Marshal.ReadIntPtr((IntPtr) num11);
                CondState.SCDefinitions[j] = Marshal.PtrToStringUni(ptr4);
                Marshal.FreeCoTaskMem(ptr4);
                num11 += IntPtr.Size;
            }
            Marshal.FreeCoTaskMem(ptr5);
            num10 += IntPtr.Size;
            CondState.SCSeverities = new int[CondState.NumSCs];
            ptr5 = Marshal.ReadIntPtr((IntPtr) num10);
            num11 = ptr5.ToInt32();
            for (int k = 0; k < CondState.NumSCs; k++)
            {
                CondState.SCSeverities[k] = Marshal.ReadInt32((IntPtr) num11);
                num11 += 4L;
            }
            Marshal.FreeCoTaskMem(ptr5);
            num10 += IntPtr.Size;
            CondState.SCDescriptions = new string[CondState.NumSCs];
            ptr5 = Marshal.ReadIntPtr((IntPtr) num10);
            num11 = ptr5.ToInt32();
            for (int m = 0; m < CondState.NumSCs; m++)
            {
                ptr4 = Marshal.ReadIntPtr((IntPtr) num11);
                CondState.SCDescriptions[m] = Marshal.PtrToStringUni(ptr4);
                Marshal.FreeCoTaskMem(ptr4);
                num11 += IntPtr.Size;
            }
            Marshal.FreeCoTaskMem(ptr5);
            num10 += IntPtr.Size;
            CondState.NumEventAttrs = Marshal.ReadInt32((IntPtr) num10);
            num10 += 8L;
            CondState.EventAttributes = new object[CondState.NumEventAttrs];
            ptr5 = Marshal.ReadIntPtr((IntPtr) num10);
            num11 = ptr5.ToInt32();
            for (int n = 0; n < CondState.NumEventAttrs; n++)
            {
                CondState.EventAttributes[n] = Marshal.GetObjectForNativeVariant((IntPtr) num11);
                DUMMY_VARIANT.VariantClear((IntPtr) num11);
                num11 += DUMMY_VARIANT.ConstSize;
            }
            Marshal.FreeCoTaskMem(ptr5);
            num10 += IntPtr.Size;
            CondState.Errors = new int[CondState.NumEventAttrs];
            ptr5 = Marshal.ReadIntPtr((IntPtr) num10);
            num11 = ptr5.ToInt32();
            for (int num17 = 0; num17 < CondState.NumEventAttrs; num17++)
            {
                CondState.Errors[num17] = Marshal.ReadInt32((IntPtr) num11);
                num11 += 4L;
            }
            Marshal.FreeCoTaskMem(ptr5);
            num10 += IntPtr.Size;
            return hresultcode;
        }

        public int GetEnableStateByArea(string[] Areas, out bool[] Enabled, out bool[] EffectivelyEnabled, out int[] Errors)
        {
            Errors = null;
            Enabled = null;
            EffectivelyEnabled = null;
            if (this.ifAEServer2 == null)
            {
                if (myErrorsAsExecptions)
                {
                    throw new OPCException(-2147467263, ErrorDescriptions.GetErrorDescription(-2147467263));
                }
                return -2147467263;
            }
            IntPtr ptr; IntPtr ptr2; IntPtr ptr3;
            int hresultcode = this.ifAEServer2.GetEnableStateByArea(Areas.Length, Areas, out ptr, out ptr2, out ptr3);
            if (HRESULTS.Failed(hresultcode))
            {
                if (myErrorsAsExecptions)
                {
                    throw new OPCException(hresultcode, ErrorDescriptions.GetErrorDescription(hresultcode));
                }
                return hresultcode;
            }
            long num2 = (long) ptr;
            long num3 = (long) ptr2;
            long num4 = (long) ptr3;
            if (((num2 != 0L) && (num3 != 0L)) && (num4 != 0L))
            {
                Enabled = new bool[Areas.Length];
                EffectivelyEnabled = new bool[Areas.Length];
                Errors = new int[Areas.Length];
                for (int i = 0; i < Areas.Length; i++)
                {
                    Enabled[i] = Marshal.ReadInt32((IntPtr) num2) != 0;
                    num2 += 4L;
                    EffectivelyEnabled[i] = Marshal.ReadInt32((IntPtr) num3) != 0;
                    num3 += 4L;
                    Errors[i] = Marshal.ReadInt32((IntPtr) num4);
                    num4 += 4L;
                }
                Marshal.FreeCoTaskMem(ptr);
                Marshal.FreeCoTaskMem(ptr2);
                Marshal.FreeCoTaskMem(ptr3);
                return hresultcode;
            }
            if (myErrorsAsExecptions)
            {
                throw new OPCException(-2147467259, ErrorDescriptions.GetErrorDescription(-2147467259));
            }
            return -2147467259;
        }

        public int GetEnableStateBySource(string[] Sources, out bool[] Enabled, out bool[] EffectivelyEnabled, out int[] Errors)
        {
            Errors = null;
            Enabled = null;
            EffectivelyEnabled = null;
            if (this.ifAEServer2 == null)
            {
                if (myErrorsAsExecptions)
                {
                    throw new OPCException(-2147467263, ErrorDescriptions.GetErrorDescription(-2147467263));
                }
                return -2147467263;
            }
            IntPtr ptr; IntPtr ptr2; IntPtr ptr3;
            int hresultcode = this.ifAEServer2.GetEnableStateByArea(Sources.Length, Sources, out ptr, out ptr2, out ptr3);
            if (HRESULTS.Failed(hresultcode))
            {
                if (myErrorsAsExecptions)
                {
                    throw new OPCException(hresultcode, ErrorDescriptions.GetErrorDescription(hresultcode));
                }
                return hresultcode;
            }
            long num2 = (long) ptr;
            long num3 = (long) ptr2;
            long num4 = (long) ptr3;
            if (((num2 != 0L) && (num3 != 0L)) && (num4 != 0L))
            {
                Enabled = new bool[Sources.Length];
                EffectivelyEnabled = new bool[Sources.Length];
                Errors = new int[Sources.Length];
                for (int i = 0; i < Sources.Length; i++)
                {
                    Enabled[i] = Marshal.ReadInt32((IntPtr) num2) != 0;
                    num2 += 4L;
                    EffectivelyEnabled[i] = Marshal.ReadInt32((IntPtr) num3) != 0;
                    num3 += 4L;
                    Errors[i] = Marshal.ReadInt32((IntPtr) num4);
                    num4 += 4L;
                }
                Marshal.FreeCoTaskMem(ptr);
                Marshal.FreeCoTaskMem(ptr2);
                Marshal.FreeCoTaskMem(ptr3);
                return hresultcode;
            }
            if (myErrorsAsExecptions)
            {
                throw new OPCException(-2147467259, ErrorDescriptions.GetErrorDescription(-2147467259));
            }
            return -2147467259;
        }

        public string GetErrorString(int errorCode)
        {
            string ppString = null;
            if (HRESULTS.Failed(this.ifCommon.GetErrorString(errorCode, out ppString)))
            {
                ppString = ErrorDescriptions.GetErrorDescription(errorCode);
            }
            return ppString;
        }

        public int GetLocaleID(out int lcid)
        {
            int localeID = this.ifCommon.GetLocaleID(out lcid);
            if (myErrorsAsExecptions && HRESULTS.Failed(localeID))
            {
                throw new OPCException(localeID, ErrorDescriptions.GetErrorDescription(localeID));
            }
            return localeID;
        }

        public int GetStatus(out EventServerStatus srvStatus)
        {
            srvStatus = new EventServerStatus();
            IntPtr ptr;
            int status = this.ifAEServer.GetStatus(out ptr);
            if (HRESULTS.Succeeded(status))
            {
                IntPtr ptr2;
                long num2 = (long) ptr;
                srvStatus.StartTime = DateTime.FromFileTime(Marshal.ReadInt64(ptr));
                srvStatus.CurrentTime = DateTime.FromFileTime(Marshal.ReadInt64((IntPtr) (num2 + 8L)));
                srvStatus.LastUpdateTime = DateTime.FromFileTime(Marshal.ReadInt64((IntPtr) (num2 + 0x10L)));
                srvStatus.ServerState = (OPCEventServerState) Marshal.ReadInt32((IntPtr) (num2 + 0x18L));
                srvStatus.MajorVersion = Marshal.ReadInt16((IntPtr) (num2 + 0x1cL));
                srvStatus.MinorVersion = Marshal.ReadInt16((IntPtr) (num2 + 30L));
                srvStatus.BuildNumber = Marshal.ReadInt16((IntPtr) (num2 + 0x20L));
                if (IntPtr.Size < 8)
                {
                    ptr2 = Marshal.ReadIntPtr((IntPtr) (num2 + 0x24L));
                }
                else
                {
                    ptr2 = Marshal.ReadIntPtr((IntPtr) (num2 + 40L));
                }
                srvStatus.VendorInfo = Marshal.PtrToStringUni(ptr2);
                Marshal.FreeCoTaskMem(ptr2);
            }
            if (myErrorsAsExecptions && HRESULTS.Failed(status))
            {
                throw new OPCException(status, ErrorDescriptions.GetErrorDescription(status));
            }
            return status;
        }

        public int IsAvailableNT(out bool available)
        {
            available = false;
            if (this.ifSecurityNT == null)
            {
                if (myErrorsAsExecptions)
                {
                    throw new OPCException(-2147467263, ErrorDescriptions.GetErrorDescription(-2147467263));
                }
                return -2147467263;
            }
            int hresultcode = this.ifSecurityNT.IsAvailableNT(out available);
            if (myErrorsAsExecptions && HRESULTS.Failed(hresultcode))
            {
                throw new OPCException(hresultcode, ErrorDescriptions.GetErrorDescription(hresultcode));
            }
            return hresultcode;
        }

        public int IsAvailablePriv(out bool available)
        {
            available = false;
            if (this.ifSecurityPriv == null)
            {
                if (myErrorsAsExecptions)
                {
                    throw new OPCException(-2147467263, ErrorDescriptions.GetErrorDescription(-2147467263));
                }
                return -2147467263;
            }
            int hresultcode = this.ifSecurityPriv.IsAvailablePriv(out available);
            if (myErrorsAsExecptions && HRESULTS.Failed(hresultcode))
            {
                throw new OPCException(hresultcode, ErrorDescriptions.GetErrorDescription(hresultcode));
            }
            return hresultcode;
        }

        public int Logoff()
        {
            if (this.ifSecurityPriv == null)
            {
                if (myErrorsAsExecptions)
                {
                    throw new OPCException(-2147467263, ErrorDescriptions.GetErrorDescription(-2147467263));
                }
                return -2147467263;
            }
            int hresultcode = this.ifSecurityPriv.Logoff();
            if (myErrorsAsExecptions && HRESULTS.Failed(hresultcode))
            {
                throw new OPCException(hresultcode, ErrorDescriptions.GetErrorDescription(hresultcode));
            }
            return hresultcode;
        }

        public int Logon(string userID, string password)
        {
            if (this.ifSecurityPriv == null)
            {
                if (myErrorsAsExecptions)
                {
                    throw new OPCException(-2147467263, ErrorDescriptions.GetErrorDescription(-2147467263));
                }
                return -2147467263;
            }
            int hresultcode = this.ifSecurityPriv.Logon(userID, password);
            if (myErrorsAsExecptions && HRESULTS.Failed(hresultcode))
            {
                throw new OPCException(hresultcode, ErrorDescriptions.GetErrorDescription(hresultcode));
            }
            return hresultcode;
        }

        void IOPCShutdown.ShutdownRequest(string shutdownReason)
        {
            ShutdownRequestEventArgs e = new ShutdownRequestEventArgs(shutdownReason);
            if (this.ShutdownRequested != null)
            {
                this.ShutdownRequested(this, e);
            }
        }

        public int QueryAvailableFilters(out OPCAEFilters FilterMask)
        {
            int hresultcode = this.ifAEServer.QueryAvailableFilters(out FilterMask);
            if (myErrorsAsExecptions && HRESULTS.Failed(hresultcode))
            {
                throw new OPCException(hresultcode, ErrorDescriptions.GetErrorDescription(hresultcode));
            }
            return hresultcode;
        }

        public int QueryAvailableLocaleIDs(out int[] lcids)
        {
            lcids = null;
            int num;
            IntPtr ptr;
            int hresultcode = this.ifCommon.QueryAvailableLocaleIDs(out num, out ptr);
            if (HRESULTS.Failed(hresultcode))
            {
                if (myErrorsAsExecptions)
                {
                    throw new OPCException(hresultcode, ErrorDescriptions.GetErrorDescription(hresultcode));
                }
                return hresultcode;
            }
            if (((int) ptr) == 0)
            {
                if (myErrorsAsExecptions)
                {
                    throw new OPCException(-2147467259, ErrorDescriptions.GetErrorDescription(-2147467259));
                }
                return -2147467259;
            }
            if (num < 1)
            {
                Marshal.FreeCoTaskMem(ptr);
                if (myErrorsAsExecptions)
                {
                    throw new OPCException(-2147467259, ErrorDescriptions.GetErrorDescription(-2147467259));
                }
                return -2147467259;
            }
            lcids = new int[num];
            Marshal.Copy(ptr, lcids, 0, num);
            Marshal.FreeCoTaskMem(ptr);
            return 0;
        }

        public int QueryConditionNames(int EventCategory, out string[] ConditionNames)
        {
            ConditionNames = null;
            int num; IntPtr ptr;
            int hresultcode = this.ifAEServer.QueryConditionNames(EventCategory, out num, out ptr);
            if (HRESULTS.Failed(hresultcode))
            {
                if (myErrorsAsExecptions)
                {
                    throw new OPCException(hresultcode, ErrorDescriptions.GetErrorDescription(hresultcode));
                }
                return hresultcode;
            }
            if (num == 0)
            {
                return hresultcode;
            }
            long num3 = (long) ptr;
            if (num3 == 0L)
            {
                if (myErrorsAsExecptions)
                {
                    throw new OPCException(-2147467259, ErrorDescriptions.GetErrorDescription(-2147467259));
                }
                return -2147467259;
            }
            ConditionNames = new string[num];
            for (int i = 0; i < num; i++)
            {
                IntPtr ptr2 = Marshal.ReadIntPtr((IntPtr) num3);
                num3 += IntPtr.Size;
                ConditionNames[i] = Marshal.PtrToStringUni(ptr2);
                Marshal.FreeCoTaskMem(ptr2);
            }
            Marshal.FreeCoTaskMem(ptr);
            return 0;
        }

        public int QueryEventAttributes(int EventCategory, out int[] ppdwAttrIDs, out string[] ppszAttrDescs, out VarEnum[] ppvtAttrTypes)
        {
            ppdwAttrIDs = null;
            ppszAttrDescs = null;
            ppvtAttrTypes = null;
            int num; IntPtr ptr; IntPtr ptr2; IntPtr ptr3;
            int hresultcode = this.ifAEServer.QueryEventAttributes(EventCategory, out num, out ptr, out ptr2, out ptr3);
            if (HRESULTS.Failed(hresultcode))
            {
                if (myErrorsAsExecptions)
                {
                    throw new OPCException(hresultcode, ErrorDescriptions.GetErrorDescription(hresultcode));
                }
                return hresultcode;
            }
            if (num == 0)
            {
                return hresultcode;
            }
            long num3 = (long) ptr;
            long num4 = (long) ptr2;
            long num5 = (long) ptr3;
            if (((num3 != 0L) && (num4 != 0L)) && (num5 != 0L))
            {
                ppdwAttrIDs = new int[num];
                ppszAttrDescs = new string[num];
                ppvtAttrTypes = new VarEnum[num];
                for (int i = 0; i < num; i++)
                {
                    ppdwAttrIDs[i] = Marshal.ReadInt32((IntPtr) num3);
                    num3 += 4L;
                    IntPtr ptr4 = Marshal.ReadIntPtr((IntPtr) num4);
                    num4 += IntPtr.Size;
                    ppszAttrDescs[i] = Marshal.PtrToStringUni(ptr4);
                    Marshal.FreeCoTaskMem(ptr4);
                    ppvtAttrTypes[i] = (VarEnum) Marshal.ReadInt16((IntPtr) num5);
                    num5 += 2L;
                }
                Marshal.FreeCoTaskMem(ptr);
                Marshal.FreeCoTaskMem(ptr2);
                Marshal.FreeCoTaskMem(ptr3);
                return 0;
            }
            if (myErrorsAsExecptions)
            {
                throw new OPCException(-2147467259, ErrorDescriptions.GetErrorDescription(-2147467259));
            }
            return -2147467259;
        }

        public int QueryEventCategories(OPCAEEventType EventType, out int[] EventCategories, out string[] EventCategoryDescs)
        {
            EventCategories = null;
            EventCategoryDescs = null;
            int num; IntPtr ptr; IntPtr ptr2;
            int hresultcode = this.ifAEServer.QueryEventCategories((int) EventType, out num, out ptr, out ptr2);
            if (HRESULTS.Failed(hresultcode))
            {
                if (myErrorsAsExecptions)
                {
                    throw new OPCException(hresultcode, ErrorDescriptions.GetErrorDescription(hresultcode));
                }
                return hresultcode;
            }
            if (num == 0)
            {
                return hresultcode;
            }
            long num3 = (long) ptr;
            long num4 = (long) ptr2;
            if ((num3 != 0L) && (num4 != 0L))
            {
                EventCategories = new int[num];
                EventCategoryDescs = new string[num];
                for (int i = 0; i < num; i++)
                {
                    EventCategories[i] = Marshal.ReadInt32((IntPtr) num3);
                    num3 += 4L;
                    IntPtr ptr3 = Marshal.ReadIntPtr((IntPtr) num4);
                    num4 += IntPtr.Size;
                    EventCategoryDescs[i] = Marshal.PtrToStringUni(ptr3);
                    Marshal.FreeCoTaskMem(ptr3);
                }
                Marshal.FreeCoTaskMem(ptr);
                Marshal.FreeCoTaskMem(ptr2);
                return 0;
            }
            if (myErrorsAsExecptions)
            {
                throw new OPCException(-2147467259, ErrorDescriptions.GetErrorDescription(-2147467259));
            }
            return -2147467259;
        }

        public int QueryMinImpersonationLevel(out int minImpLevel)
        {
            minImpLevel = 0;
            if (this.ifSecurityNT == null)
            {
                if (myErrorsAsExecptions)
                {
                    throw new OPCException(-2147467263, ErrorDescriptions.GetErrorDescription(-2147467263));
                }
                return -2147467263;
            }
            int hresultcode = this.ifSecurityNT.QueryMinImpersonationLevel(out minImpLevel);
            if (myErrorsAsExecptions && HRESULTS.Failed(hresultcode))
            {
                throw new OPCException(hresultcode, ErrorDescriptions.GetErrorDescription(hresultcode));
            }
            return hresultcode;
        }

        public int QuerySourceConditions(string Source, out string[] ConditionNames)
        {
            ConditionNames = null;
            int num;
            IntPtr ptr;
            int hresultcode = this.ifAEServer.QuerySourceConditions(Source, out num, out ptr);
            if (HRESULTS.Failed(hresultcode))
            {
                if (myErrorsAsExecptions)
                {
                    throw new OPCException(hresultcode, ErrorDescriptions.GetErrorDescription(hresultcode));
                }
                return hresultcode;
            }
            if (num == 0)
            {
                return hresultcode;
            }
            long num3 = (long) ptr;
            if (num3 == 0L)
            {
                if (myErrorsAsExecptions)
                {
                    throw new OPCException(-2147467259, ErrorDescriptions.GetErrorDescription(-2147467259));
                }
                return -2147467259;
            }
            ConditionNames = new string[num];
            for (int i = 0; i < num; i++)
            {
                IntPtr ptr2 = Marshal.ReadIntPtr((IntPtr) num3);
                num3 += IntPtr.Size;
                ConditionNames[i] = Marshal.PtrToStringUni(ptr2);
                Marshal.FreeCoTaskMem(ptr2);
            }
            Marshal.FreeCoTaskMem(ptr);
            return 0;
        }

        public int QuerySubConditionNames(string ConditionName, out string[] SubConditionNames)
        {
            SubConditionNames = null;
            int num; IntPtr ptr;
            int hresultcode = this.ifAEServer.QuerySubConditionNames(ConditionName, out num, out ptr);
            if (HRESULTS.Failed(hresultcode))
            {
                if (myErrorsAsExecptions)
                {
                    throw new OPCException(hresultcode, ErrorDescriptions.GetErrorDescription(hresultcode));
                }
                return hresultcode;
            }
            if (num == 0)
            {
                return hresultcode;
            }
            long num3 = (long) ptr;
            if (num3 == 0L)
            {
                if (myErrorsAsExecptions)
                {
                    throw new OPCException(-2147467259, ErrorDescriptions.GetErrorDescription(-2147467259));
                }
                return -2147467259;
            }
            SubConditionNames = new string[num];
            for (int i = 0; i < num; i++)
            {
                IntPtr ptr2 = Marshal.ReadIntPtr((IntPtr) num3);
                num3 += IntPtr.Size;
                SubConditionNames[i] = Marshal.PtrToStringUni(ptr2);
                Marshal.FreeCoTaskMem(ptr2);
            }
            Marshal.FreeCoTaskMem(ptr);
            return 0;
        }

        private void RegisterAppCallbackRights()
        {
            try
            {
                string str2;
                string location = Assembly.GetEntryAssembly().Location;
                int num = location.LastIndexOf(@"\");
                if (num >= 0)
                {
                    str2 = location.Substring(num + 1);
                }
                else
                {
                    str2 = location.Substring(0);
                }
                string str3 = str2.Substring(0, str2.LastIndexOf("."));
                RegistryKey classesRoot = Registry.ClassesRoot;
                RegistryKey appid = classesRoot.OpenSubKey("AppID", true);
                RegistryKey key3 = this.FindAppExe(appid, str2);
                if (key3 == null)
                {
                    Guid guid = Guid.NewGuid();
                    string str4 = "{" + guid.ToString().ToUpper() + "}";
                    key3 = appid.CreateSubKey(str2);
                    key3.SetValue("AppID", str4);
                    RegistryKey key4 = appid.CreateSubKey(str4);
                    key4.SetValue("AuthenticationLevel", 1);
                    key4.SetValue("", str3);
                }
                key3.Close();
                appid.Close();
                classesRoot.Close();
                if (Register.ServerObject(this))
                {
                    throw new OPCException(-2147220992, "Trial period has expired");
                }
            }
            catch (Exception exception)
            {
                string message = exception.Message;
            }
        }

        public int SetClientName(string name)
        {
            int hresultcode = this.ifCommon.SetClientName(name);
            if (myErrorsAsExecptions && HRESULTS.Failed(hresultcode))
            {
                throw new OPCException(hresultcode, ErrorDescriptions.GetErrorDescription(hresultcode));
            }
            return hresultcode;
        }

        private int SetInterfaces(object srvObj)
        {
            if (Register.ServerObject(this))
            {
                throw new OPCException(-2147220992, "Trial period has expired");
            }
            this.OPCeventServerObj = srvObj;
            this.ifAEServer = (IOPCEventServer) this.OPCeventServerObj;
            if (this.ifAEServer == null)
            {
                if (myErrorsAsExecptions)
                {
                    throw new OPCException(-2147220992, ErrorDescriptions.GetErrorDescription(-2147220992));
                }
                return -2147220992;
            }
            try
            {
                this.ifAEServer2 = (IOPCEventServer2) this.OPCeventServerObj;
            }
            catch
            {
                this.ifAEServer2 = null;
            }
            try
            {
                this.ifCommon = (IOPCCommon) this.OPCeventServerObj;
            }
            catch
            {
            }
            try
            {
                this.ifSecurityNT = (IOPCSecurityNT) this.OPCeventServerObj;
            }
            catch
            {
            }
            try
            {
                this.ifSecurityPriv = (IOPCSecurityPrivate) this.OPCeventServerObj;
            }
            catch
            {
            }
            try
            {
                this.evCpointcontainer = (UCOMIConnectionPointContainer) this.OPCeventServerObj;
                this.AdviseIOPCShutdown();
            }
            catch
            {
            }
            return 0;
        }

        public int SetLocaleID(int lcid)
        {
            int hresultcode = this.ifCommon.SetLocaleID(lcid);
            if (myErrorsAsExecptions && HRESULTS.Failed(hresultcode))
            {
                throw new OPCException(hresultcode, ErrorDescriptions.GetErrorDescription(hresultcode));
            }
            return hresultcode;
        }

        public int TranslateToItemIDs(string Source, int EventCategory, string ConditionName, string SubconditionName, int[] AssocAttrIDs, out string[] AttrItemIDs, out string[] NodeNames, out Guid[] CLSIDs)
        {
            AttrItemIDs = null;
            NodeNames = null;
            CLSIDs = null;
            IntPtr ptr; IntPtr ptr2; IntPtr ptr3;
            int hresultcode = this.ifAEServer.TranslateToItemIDs(Source, EventCategory, ConditionName, SubconditionName, AssocAttrIDs.Length, AssocAttrIDs, out ptr, out ptr2, out ptr3);
            if (HRESULTS.Failed(hresultcode))
            {
                if (myErrorsAsExecptions)
                {
                    throw new OPCException(hresultcode, ErrorDescriptions.GetErrorDescription(hresultcode));
                }
                return hresultcode;
            }
            int length = AssocAttrIDs.Length;
            long num3 = (long) ptr;
            long num4 = (long) ptr2;
            long num5 = (long) ptr3;
            if (((num3 != 0L) && (num4 != 0L)) && (num5 != 0L))
            {
                CLSIDs = new Guid[length];
                for (int i = 0; i < length; i++)
                {
                    int a = Marshal.ReadInt32((IntPtr) num5);
                    num5 += 4L;
                    short b = Marshal.ReadInt16((IntPtr) num5);
                    num5 += 2L;
                    short c = Marshal.ReadInt16((IntPtr) num5);
                    num5 += 2L;
                    byte[] buffer = new byte[8];
                    for (int m = 0; m < 8; m++)
                    {
                        buffer[i] = Marshal.ReadByte((IntPtr) num5);
                        num5 += 1L;
                    }
                    CLSIDs[i] = new Guid(a, b, c, buffer[0], buffer[1], buffer[2], buffer[3], buffer[4], buffer[5], buffer[6], buffer[7]);
                }
                AttrItemIDs = new string[length];
                for (int j = 0; j < length; j++)
                {
                    IntPtr ptr4 = Marshal.ReadIntPtr((IntPtr) num3);
                    num3 += IntPtr.Size;
                    AttrItemIDs[j] = Marshal.PtrToStringUni(ptr4);
                    Marshal.FreeCoTaskMem(ptr4);
                }
                NodeNames = new string[length];
                for (int k = 0; k < length; k++)
                {
                    IntPtr ptr5 = Marshal.ReadIntPtr((IntPtr) num4);
                    num4 += IntPtr.Size;
                    NodeNames[k] = Marshal.PtrToStringUni(ptr5);
                    Marshal.FreeCoTaskMem(ptr5);
                }
                if (num3 != 0L)
                {
                    Marshal.FreeCoTaskMem(ptr);
                }
                if (num4 != 0L)
                {
                    Marshal.FreeCoTaskMem(ptr2);
                }
                if (num5 != 0L)
                {
                    Marshal.FreeCoTaskMem(ptr3);
                }
                if (myErrorsAsExecptions && HRESULTS.Failed(hresultcode))
                {
                    throw new OPCException(hresultcode, ErrorDescriptions.GetErrorDescription(hresultcode));
                }
                return hresultcode;
            }
            if (myErrorsAsExecptions)
            {
                throw new OPCException(-2147467259, ErrorDescriptions.GetErrorDescription(-2147467259));
            }
            return -2147467259;
        }

        private static string WrapperVersion()
        {
            string fileVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
            int length = fileVersion.LastIndexOf('.');
            return fileVersion.Substring(0, length);
        }

        public static string Version =>
            WrapperVersion();

        public string ServerName =>
            this.myServerName;

        public Host HostInfo =>
            this.myHostInfo;

        public bool ConnectThroughNIOS
        {
            get {
                return this.myConnectThroughNIOS;
            }
            set
            {
                this.myConnectThroughNIOS = value;
            }
        }

        public static bool ErrorsAsExecptions
        {
            get
            {
                return myErrorsAsExecptions;
            }
            set  
            { 
                myErrorsAsExecptions = value; 
            }
        }
    }
}

