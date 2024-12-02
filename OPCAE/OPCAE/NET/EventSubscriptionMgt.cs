using OPCAE.OPC;
using OPCAE.OPCAE.Contract;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;

namespace OPCAE.OPCAE.NET
{
    [SuppressUnmanagedCodeSecurity, ComVisible(true), ReflectionPermission(SecurityAction.Assert, Unrestricted=true), SecurityPermission(SecurityAction.Assert, UnmanagedCode=true)]
    public class EventSubscriptionMgt : IOPCEventSink
    {
        private Guid riidSubscrMgt = new Guid("{65168855-5783-11D1-84A0-00608CB8A7E9}");
        private IOPCEventSubscriptionMgt ifSubscrMgt;
        private IOPCEventSubscriptionMgt2 ifSubscrMgt2 = null;
        private UCOMIConnectionPointContainer cpointcontainer = null;
        private UCOMIConnectionPoint callbackcpoint = null;
        private int callbackcookie = 0;
        private OpcEventServer Srv;

        private event OnAEeventHandler ClientHandler;

        public EventSubscriptionMgt(OnAEeventHandler clh)
        {
            this.ClientHandler = (OnAEeventHandler) Delegate.Combine(this.ClientHandler, new OnAEeventHandler(clh.Invoke));
        }

        public void AdviseIOPCEventSink()
        {
            Type type = typeof(IOPCEventSink);
            Guid gUID = type.GUID;
            this.cpointcontainer.FindConnectionPoint(ref gUID, out this.callbackcpoint);
            if (this.callbackcpoint != null)
            {
               // try
                //{
                    this.callbackcpoint.Advise(this, out this.callbackcookie);
                //}catch (Exception ex)
               // {

                //}
                
            }
        }

        public int CancelRefresh()
        {
            int hresultcode = this.ifSubscrMgt.CancelRefresh(this.callbackcookie);
            if (OpcEventServer.myErrorsAsExecptions && HRESULTS.Failed(hresultcode))
            {
                throw new OPCException(hresultcode, ErrorDescriptions.GetErrorDescription(hresultcode));
            }
            return hresultcode;
        }

        public int Create(OpcEventServer srv, bool Active, int BufferTime, int MaxSize, int ClientSubscription, out int RevisedBufferTime, out int RevisedMaxSize)
        {
            this.Srv = srv;
            this.ifSubscrMgt = null;
            object ppUnk = null;
            int hresultcode = srv.CreateEventSubscription(Active, BufferTime, MaxSize, ClientSubscription, ref this.riidSubscrMgt, out ppUnk, out RevisedBufferTime, out RevisedMaxSize);
            if (HRESULTS.Succeeded(hresultcode))
            {
                this.ifSubscrMgt = (IOPCEventSubscriptionMgt) ppUnk;
                try
                {
                    this.ifSubscrMgt2 = (IOPCEventSubscriptionMgt2) ppUnk;
                }
                catch
                {
                }
                ppUnk = null;
                this.cpointcontainer = (UCOMIConnectionPointContainer) this.ifSubscrMgt;
                this.AdviseIOPCEventSink();
            }
            if (OpcEventServer.myErrorsAsExecptions && HRESULTS.Failed(hresultcode))
            {
                throw new OPCException(hresultcode, ErrorDescriptions.GetErrorDescription(hresultcode));
            }
            return hresultcode;
        }

        public void Dispose()
        {
            try
            {
                if (this.callbackcpoint != null)
                {
                    if (this.callbackcookie != 0)
                    {
                        this.callbackcpoint.Unadvise(this.callbackcookie);
                        this.callbackcookie = 0;
                    }
                    Marshal.ReleaseComObject(this.callbackcpoint);
                    this.callbackcpoint = null;
                }
                if (this.ifSubscrMgt != null)
                {
                    Marshal.ReleaseComObject(this.ifSubscrMgt);
                }
                if (this.ifSubscrMgt2 != null)
                {
                    Marshal.ReleaseComObject(this.ifSubscrMgt2);
                }
            }
            catch
            {
            }
        }

        ~EventSubscriptionMgt()
        {
            this.Dispose();
        }

        public int GetFilter(out int EventType, out int[] EventCategories, out int LowSeverity, out int HighSeverity, out string[] AreaList, out string[] SourceList)
        {
            EventCategories = null;
            AreaList = null;
            SourceList = null;
            int num;
            IntPtr ptr;
            int num2;  IntPtr ptr2;  int num3; IntPtr ptr3;
            int hresultcode = this.ifSubscrMgt.GetFilter(out EventType, out num, out ptr, out LowSeverity, out HighSeverity, out num2, out ptr2, out num3, out ptr3);
            if (HRESULTS.Failed(hresultcode))
            {
                if (OpcEventServer.myErrorsAsExecptions)
                {
                    throw new OPCException(hresultcode, ErrorDescriptions.GetErrorDescription(hresultcode));
                }
                return hresultcode;
            }
            long num5 = (long) ptr;
            long num6 = (long) ptr2;
            long num7 = (long) ptr3;
            if ((num > 0) && (num5 == 0L))
            {
                if (OpcEventServer.myErrorsAsExecptions)
                {
                    throw new OPCException(-2147467259, ErrorDescriptions.GetErrorDescription(-2147467259));
                }
                return -2147467259;
            }
            if ((num2 > 0) && (num6 == 0L))
            {
                if (OpcEventServer.myErrorsAsExecptions)
                {
                    throw new OPCException(-2147467259, ErrorDescriptions.GetErrorDescription(-2147467259));
                }
                return -2147467259;
            }
            if ((num3 > 0) && (num7 == 0L))
            {
                if (OpcEventServer.myErrorsAsExecptions)
                {
                    throw new OPCException(-2147467259, ErrorDescriptions.GetErrorDescription(-2147467259));
                }
                return -2147467259;
            }
            if (num5 != 0L)
            {
                EventCategories = new int[num];
            }
            for (int i = 0; i < num; i++)
            {
                EventCategories[i] = Marshal.ReadInt32((IntPtr) num5);
                num5 += 4L;
            }
            if (num6 != 0L)
            {
                AreaList = new string[num2];
            }
            for (int j = 0; j < num2; j++)
            {
                IntPtr ptr4 = Marshal.ReadIntPtr((IntPtr) num6);
                num6 += IntPtr.Size;
                AreaList[j] = Marshal.PtrToStringUni(ptr4);
                Marshal.FreeCoTaskMem(ptr4);
            }
            if (num7 != 0L)
            {
                SourceList = new string[num3];
            }
            for (int k = 0; k < num3; k++)
            {
                IntPtr ptr5 = Marshal.ReadIntPtr((IntPtr) num7);
                num7 += IntPtr.Size;
                SourceList[k] = Marshal.PtrToStringUni(ptr5);
                Marshal.FreeCoTaskMem(ptr5);
            }
            if (num5 != 0L)
            {
                Marshal.FreeCoTaskMem(ptr);
            }
            if (num6 != 0L)
            {
                Marshal.FreeCoTaskMem(ptr2);
            }
            if (num7 != 0L)
            {
                Marshal.FreeCoTaskMem(ptr3);
            }
            if (OpcEventServer.myErrorsAsExecptions && HRESULTS.Failed(hresultcode))
            {
                throw new OPCException(hresultcode, ErrorDescriptions.GetErrorDescription(hresultcode));
            }
            return hresultcode;
        }

        public int GetKeepAlive(out int KeepAliveTime)
        {
            KeepAliveTime = 0;
            if (this.ifSubscrMgt2 == null)
            {
                if (OpcEventServer.myErrorsAsExecptions)
                {
                    throw new OPCException(-2147467263, ErrorDescriptions.GetErrorDescription(-2147467263));
                }
                return -2147467263;
            }
            int keepAlive = this.ifSubscrMgt2.GetKeepAlive(out KeepAliveTime);
            if (OpcEventServer.myErrorsAsExecptions && HRESULTS.Failed(keepAlive))
            {
                throw new OPCException(keepAlive, ErrorDescriptions.GetErrorDescription(keepAlive));
            }
            return keepAlive;
        }

        public int GetReturnedAttributes(int EventCategory, out int[] AttributeIDs)
        {
            AttributeIDs = null;
            int num; IntPtr ptr;
            int hresultcode = this.ifSubscrMgt.GetReturnedAttributes(EventCategory, out num, out ptr);
            if (HRESULTS.Failed(hresultcode))
            {
                if (OpcEventServer.myErrorsAsExecptions)
                {
                    throw new OPCException(hresultcode, ErrorDescriptions.GetErrorDescription(hresultcode));
                }
                return hresultcode;
            }
            long num3 = (long) ptr;
            if ((num > 0) && (num3 == 0L))
            {
                if (OpcEventServer.myErrorsAsExecptions)
                {
                    throw new OPCException(-2147467259, ErrorDescriptions.GetErrorDescription(-2147467259));
                }
                return -2147467259;
            }
            if (num > 0)
            {
                AttributeIDs = new int[num];
                for (int i = 0; i < num; i++)
                {
                    AttributeIDs[i] = Marshal.ReadInt32((IntPtr) num3);
                    num3 += 4L;
                }
                Marshal.FreeCoTaskMem(ptr);
            }
            if (OpcEventServer.myErrorsAsExecptions && HRESULTS.Failed(hresultcode))
            {
                throw new OPCException(hresultcode, ErrorDescriptions.GetErrorDescription(hresultcode));
            }
            return hresultcode;
        }

        public int GetState(out bool Active, out int BufferTime, out int MaxSize, out int ClientSubscription)
        {
            int hresultcode = this.ifSubscrMgt.GetState(out Active, out BufferTime, out MaxSize, out ClientSubscription);
            if (OpcEventServer.myErrorsAsExecptions && HRESULTS.Failed(hresultcode))
            {
                throw new OPCException(hresultcode, ErrorDescriptions.GetErrorDescription(hresultcode));
            }
            return hresultcode;
        }

        int IOPCEventSink.OnEvent(int hClientSubscription, bool bRefresh, bool bLastRefresh, int dwCount, IntPtr pEvents)
        {
            userEventArgs e = new userEventArgs {
                LastRefresh = bLastRefresh,
                Refresh = bRefresh,
                ClientSubscription = hClientSubscription
            };
            if (dwCount == 0)
            {
                e.Events = null;
            }
            else
            {
                IntPtr ptr2;
                e.Events = new OnEventStruct[dwCount];
                IntPtr ptr = pEvents;
                if (IntPtr.Size < 8)
                {
                    for (int i = 0; i < dwCount; i++)
                    {
                        try
                        {
                            e.Events[i] = new OnEventStruct();
                            e.Events[i].ChangeMask = Marshal.ReadInt16(ptr);
                            e.Events[i].NewState = Marshal.ReadInt16(ptr, 2);
                            ptr2 = Marshal.ReadIntPtr(ptr, 4);
                            if (ptr2 != IntPtr.Zero)
                            {
                                e.Events[i].Source = Marshal.PtrToStringUni(ptr2);
                            }
                            
                            e.Events[i].Time = Marshal.ReadInt64(ptr, 8);
                            ptr2 = (IntPtr) Marshal.ReadInt32(ptr, 0x10);
                            if (ptr2 != IntPtr.Zero)
                            {
                                e.Events[i].Message = Marshal.PtrToStringUni(ptr2);

                                Encoding ISO88591Encoding = Encoding.GetEncoding("ISO-8859-1");
                                Encoding GB2312Encoding = Encoding.GetEncoding("gb2312"); //这个地方很特殊，必须利用GB2312编码
                                byte[] srcBytes = ISO88591Encoding.GetBytes(e.Events[i].Message);
                                //将原本存储ISO-8859-1的字节数组当成GB2312转换成目标编码(关键步骤)
                                byte[] dstBytes = Encoding.Convert(GB2312Encoding, Encoding.Default, srcBytes);
                                char[] dstChars = new char[Encoding.Default.GetCharCount(dstBytes, 0, dstBytes.Length)];
                                Encoding.Default.GetChars(dstBytes, 0, dstBytes.Length, dstChars, 0);//利用char数组存储字符
                                string sResult = new string(dstChars);
                                e.Events[i].Message = sResult;
                            }
                            e.Events[i].EventType = Marshal.ReadInt32(ptr, 20);
                            e.Events[i].EventCategory = Marshal.ReadInt32(ptr, 0x18);
                            e.Events[i].Severity = Marshal.ReadInt32(ptr, 0x1c);
                            ptr2 = Marshal.ReadIntPtr(ptr, 0x20);
                            if (ptr2 != IntPtr.Zero)
                            {
                                e.Events[i].ConditionName = Marshal.PtrToStringUni(ptr2);
                            }
                            ptr2 = Marshal.ReadIntPtr(ptr, 0x24);
                            if (ptr2 != IntPtr.Zero)
                            {
                                e.Events[i].SubconditionName = Marshal.PtrToStringUni(ptr2);
                            }
                            e.Events[i].Quality = Marshal.ReadInt16(ptr, 40);
                            e.Events[i].AckRequired = Convert.ToBoolean(Marshal.ReadInt32(ptr, 0x2c));
                            e.Events[i].ActiveTime = Marshal.ReadInt64(ptr, 0x30);
                            e.Events[i].Cookie = Marshal.ReadInt32(ptr, 0x38);
                            e.Events[i].NumEventAttrs = Marshal.ReadInt32(ptr, 60);
                            if (e.Events[i].NumEventAttrs > 0)
                            {
                                e.Events[i].EventAttributes = new object[e.Events[i].NumEventAttrs];
                                IntPtr ptr3 = Marshal.ReadIntPtr(ptr, 0x40);
                                if (ptr3 != IntPtr.Zero)
                                {
                                    int num2 = (int) ptr3;
                                    for (int j = 0; j < e.Events[i].NumEventAttrs; j++)
                                    {
                                        e.Events[i].EventAttributes[j] = Marshal.GetObjectForNativeVariant((IntPtr) num2);
                                        DUMMY_VARIANT.VariantClear((IntPtr) num2);
                                        num2 += DUMMY_VARIANT.ConstSize;
                                    }
                                }
                            }
                            ptr2 = Marshal.ReadIntPtr(ptr, 0x44);
                            if (ptr2 != IntPtr.Zero)
                            {
                                e.Events[i].ActorID = Marshal.PtrToStringUni(ptr2);
                            }
                        }
                        catch(Exception exception)
                        {
                        }
                        ptr = (IntPtr) (((int) ptr) + 0x48);
                    }
                }
                else
                {
                    for (int i = 0; i < dwCount; i++)
                    {
                        try
                        {
                            e.Events[i] = new OnEventStruct();
                            e.Events[i].ChangeMask = Marshal.ReadInt16(ptr);
                            e.Events[i].NewState = Marshal.ReadInt16(ptr, 2);
                            ptr2 = Marshal.ReadIntPtr(ptr, 8);
                            if (ptr2 != IntPtr.Zero)
                            {
                                e.Events[i].Source = Marshal.PtrToStringUni(ptr2);
                            }
                            e.Events[i].Time = Marshal.ReadInt64(ptr, 0x10);
                            ptr2 = Marshal.ReadIntPtr(ptr, 0x18);
                            if (ptr2 != IntPtr.Zero)
                            {
                                e.Events[i].Message = Marshal.PtrToStringUni(ptr2);
                            }
                            e.Events[i].EventType = Marshal.ReadInt32(ptr, 0x20);
                            e.Events[i].EventCategory = Marshal.ReadInt32(ptr, 0x24);
                            e.Events[i].Severity = Marshal.ReadInt32(ptr, 40);
                            ptr2 = Marshal.ReadIntPtr(ptr, 0x30);
                            if (ptr2 != IntPtr.Zero)
                            {
                                e.Events[i].ConditionName = Marshal.PtrToStringUni(ptr2);
                            }
                            ptr2 = Marshal.ReadIntPtr(ptr, 0x38);
                            if (ptr2 != IntPtr.Zero)
                            {
                                e.Events[i].SubconditionName = Marshal.PtrToStringUni(ptr2);
                            }
                            e.Events[i].Quality = Marshal.ReadInt16(ptr, 0x40);
                            e.Events[i].AckRequired = Convert.ToBoolean(Marshal.ReadInt32(ptr, 0x44));
                            e.Events[i].ActiveTime = Marshal.ReadInt64(ptr, 0x48);
                            e.Events[i].Cookie = Marshal.ReadInt32(ptr, 80);
                            e.Events[i].NumEventAttrs = Marshal.ReadInt32(ptr, 0x54);
                            if (e.Events[i].NumEventAttrs > 0)
                            {
                                e.Events[i].EventAttributes = new object[e.Events[i].NumEventAttrs];
                                IntPtr ptr4 = Marshal.ReadIntPtr(ptr, 0x58);
                                if (ptr4 != IntPtr.Zero)
                                {
                                    long num5 = (long) ptr4;
                                    for (int j = 0; j < e.Events[i].NumEventAttrs; j++)
                                    {
                                        e.Events[i].EventAttributes[j] = Marshal.GetObjectForNativeVariant((IntPtr) num5);
                                        DUMMY_VARIANT.VariantClear((IntPtr) num5);
                                        num5 += DUMMY_VARIANT.ConstSize;
                                    }
                                }
                            }
                            ptr2 = Marshal.ReadIntPtr(ptr, 0x60);
                            if (ptr2 != IntPtr.Zero)
                            {
                                e.Events[i].ActorID = Marshal.PtrToStringUni(ptr2);
                            }
                        }
                        catch
                        {
                        }
                        ptr = (IntPtr) (((long) ptr) + 0x68L);
                    }
                }
            }
            if ((this.ClientHandler != null) && Register.Event())
            {
                this.ClientHandler(this, e,this.Srv);
            }
            return 0;
        }

        public int Refresh()
        {
            int hresultcode = this.ifSubscrMgt.Refresh(this.callbackcookie);
            if (OpcEventServer.myErrorsAsExecptions && HRESULTS.Failed(hresultcode))
            {
                throw new OPCException(hresultcode, ErrorDescriptions.GetErrorDescription(hresultcode));
            }
            return hresultcode;
        }

        public int SelectReturnedAttributes(int EventCategory, int[] AttributeIDs)
        {
            int hresultcode = this.ifSubscrMgt.SelectReturnedAttributes(EventCategory, AttributeIDs.Length, AttributeIDs);
            if (OpcEventServer.myErrorsAsExecptions && HRESULTS.Failed(hresultcode))
            {
                throw new OPCException(hresultcode, ErrorDescriptions.GetErrorDescription(hresultcode));
            }
            return hresultcode;
        }

        public int SetFilter(int EventType, int[] EventCategories, int LowSeverity, int HighSeverity, string[] AreaList, string[] SourceList)
        {
            int length;
            //int length;
            //int length;
            if (EventCategories == null)
            {
                length = 0;
                EventCategories = new int[0];
            }
            else
            {
                length = EventCategories.Length;
            }
            if (AreaList == null)
            {
                length = 0;
                AreaList = new string[0];
            }
            else
            {
                length = AreaList.Length;
            }
            if (SourceList == null)
            {
                length = 0;
                SourceList = new string[0];
            }
            else
            {
                length = SourceList.Length;
            }
            int hresultcode = this.ifSubscrMgt.SetFilter(EventType, length, EventCategories, LowSeverity, HighSeverity, length, AreaList, length, SourceList);
            if (OpcEventServer.myErrorsAsExecptions && HRESULTS.Failed(hresultcode))
            {
                throw new OPCException(hresultcode, ErrorDescriptions.GetErrorDescription(hresultcode));
            }
            return hresultcode;
        }

        public int SetKeepAlive(int KeepAliveTime, out int RevisedKeepAliveTime)
        {
            RevisedKeepAliveTime = 0;
            if (this.ifSubscrMgt2 == null)
            {
                if (OpcEventServer.myErrorsAsExecptions)
                {
                    throw new OPCException(-2147467263, ErrorDescriptions.GetErrorDescription(-2147467263));
                }
                return -2147467263;
            }
            int hresultcode = this.ifSubscrMgt2.SetKeepAlive(KeepAliveTime, out RevisedKeepAliveTime);
            if (OpcEventServer.myErrorsAsExecptions && HRESULTS.Failed(hresultcode))
            {
                throw new OPCException(hresultcode, ErrorDescriptions.GetErrorDescription(hresultcode));
            }
            return hresultcode;
        }

        public int SetState(bool Active, int BufferTime, int MaxSize, int ClientSubscription, out int RevisedBufferTime, out int RevisedMaxSize)
        {
            int[] pbActive = new int[] { Convert.ToInt32(Active) };
            int[] pdwBufferTime = new int[] { BufferTime };
            int[] pdwMaxSize = new int[] { MaxSize };
            int hresultcode = this.ifSubscrMgt.SetState(pbActive, pdwBufferTime, pdwMaxSize, ClientSubscription, out RevisedBufferTime, out RevisedMaxSize);
            if (OpcEventServer.myErrorsAsExecptions && HRESULTS.Failed(hresultcode))
            {
                throw new OPCException(hresultcode, ErrorDescriptions.GetErrorDescription(hresultcode));
            }
            return hresultcode;
        }
    }
}

