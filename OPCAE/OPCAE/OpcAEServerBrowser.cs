using OPCAE.OPC;
using OPCAE.OPC.Common;
using OPCAE.OPCAE.NET;
using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
namespace OPCAE.OPCAE
{

    [ComVisible(true), SuppressUnmanagedCodeSecurity, ReflectionPermission(SecurityAction.Assert, Unrestricted=true), SecurityPermission(SecurityAction.Assert, UnmanagedCode=true)]
    public class OpcAEServerBrowser
    {
        private IOPCServerList ifSrvList;
        private Guid CLSID_OPCEnum;

        public OpcAEServerBrowser()
        {
            this.ifSrvList = null;
            this.CLSID_OPCEnum = new Guid("13486D51-4821-11D2-A494-3CB306C10000");
            Type typeFromCLSID = Type.GetTypeFromCLSID(this.CLSID_OPCEnum);
            this.ifSrvList = (IOPCServerList) Activator.CreateInstance(typeFromCLSID);
        }

        public OpcAEServerBrowser(Host host)
        {
            this.ifSrvList = null;
            this.CLSID_OPCEnum = new Guid("{13486D51-4821-11D2-A494-3CB306C10000}");
            if ((host.HostName != null) && (host.HostName != ""))
            {
                this.ifSrvList = (IOPCServerList) ComApi.CreateInstance(this.CLSID_OPCEnum, host);
            }
            else
            {
                Type typeFromCLSID = Type.GetTypeFromCLSID(this.CLSID_OPCEnum);
                this.ifSrvList = (IOPCServerList) Activator.CreateInstance(typeFromCLSID);
            }
        }

        public OpcAEServerBrowser(string ComputerName)
        {
            this.ifSrvList = null;
            this.CLSID_OPCEnum = new Guid("{13486D51-4821-11D2-A494-3CB306C10000}");
            if ((ComputerName != null) && (ComputerName != ""))
            {
                Host host = new Host {
                    HostName = ComputerName
                };
                this.ifSrvList = (IOPCServerList)ComApi.CreateInstance(this.CLSID_OPCEnum, host);
            }
            else
            {
                Type typeFromCLSID = Type.GetTypeFromCLSID(this.CLSID_OPCEnum);
                this.ifSrvList = (IOPCServerList) Activator.CreateInstance(typeFromCLSID);
            }
        }

        public int CLSIDFromProgID(string progId, out Guid clsid)
        {
            int hresultcode = this.ifSrvList.CLSIDFromProgID(progId, out clsid);
            if (OpcEventServer.myErrorsAsExecptions && HRESULTS.Failed(hresultcode))
            {
                throw new OPCException(hresultcode, ErrorDescriptions.GetErrorDescription(hresultcode));
            }
            return hresultcode;
        }

        public int EnumClassesOfCategories(int catListLength, Guid[] catList, int reqListLenght, Guid[] reqList, out object enumtemp)
        {
            int hresultcode = this.ifSrvList.EnumClassesOfCategories(catListLength, catList, reqListLenght, reqList, out enumtemp);
            if (OpcEventServer.myErrorsAsExecptions && HRESULTS.Failed(hresultcode))
            {
                throw new OPCException(hresultcode, ErrorDescriptions.GetErrorDescription(hresultcode));
            }
            return hresultcode;
        }

        ~OpcAEServerBrowser()
        {
            if (this.ifSrvList != null)
            {
                Marshal.ReleaseComObject(this.ifSrvList);
                this.ifSrvList = null;
            }
        }

        public int GetClassDetails(ref Guid clsid, out string progID, out string userType)
        {
            int hresultcode = this.ifSrvList.GetClassDetails(ref clsid, out progID, out userType);
            if (OpcEventServer.myErrorsAsExecptions && HRESULTS.Failed(hresultcode))
            {
                throw new OPCException(hresultcode, ErrorDescriptions.GetErrorDescription(hresultcode));
            }
            return hresultcode;
        }

        public void GetServerList(out string[] Servers)
        {
            Guid[] catList = new Guid[] { new Guid("{58E13251-AC87-11d1-84D5-00608CB8A7E9}") };
            this.GetServerList(catList, out Servers);
        }

        public void GetServerList(out string[] Servers, out Guid[] ClsIDs)
        {
            Guid[] catList = new Guid[] { new Guid("{58E13251-AC87-11d1-84D5-00608CB8A7E9}") };
            this.GetServerList(catList, out Servers, out ClsIDs);
        }

        public void GetServerList(Guid[] catList, out string[] Servers)
        {
            Guid[] guidArray;
            this.GetServerList(catList, out Servers, out guidArray);
            guidArray = null;
        }

        public void GetServerList(Guid[] catList, out string[] Servers, out Guid[] ClsIDs)
        {
            Servers = null;
            ClsIDs = null;
            if (this.ifSrvList == null)
            {
                //return null;
                //throw new OPCException(-2147467262);
            }
            else
            {
                object obj2;
                this.ifSrvList.EnumClassesOfCategories(catList.Length, catList, 0, null, out obj2);
                if (obj2 != null)
                {
                    IEnumGUID o = (IEnumGUID)obj2;
                    obj2 = null;
                    o.Reset();
                    int index = 0;
                    Guid rgelt = this.CLSID_OPCEnum;
                    Guid[] guidArray = new Guid[50];
                    int pceltFetched = 0;
                    while (true)
                    {
                        o.Next(1, ref rgelt, out pceltFetched);
                        if (pceltFetched > 0)
                        {
                            guidArray[index] = rgelt;
                            index++;
                        }
                        if ((pceltFetched <= 0) || (index >= guidArray.Length))
                        {
                            Marshal.ReleaseComObject(o);
                            o = null;
                            string[] strArray = new string[index];
                            Guid[] guidArray2 = new Guid[index];
                            int num3 = 0;
                            int num4 = 0;
                            while (true)
                            {
                                if (num4 >= index)
                                {
                                    break;
                                }
                                string ppszProgID = null;
                                string ppszUserType = null;
                                try
                                {
                                    this.ifSrvList.GetClassDetails(ref guidArray[num4], out ppszProgID, out ppszUserType);
                                    strArray[num3] = ppszProgID;
                                    guidArray2[num3] = guidArray[num4];
                                    num3++;
                                }
                                catch
                                {
                                }
                                num4++;
                            }
                            if (num3 > 0)
                            {
                                Servers = new string[num3];
                                ClsIDs = new Guid[num3];
                                for (int i = 0; i < num3; i++)
                                {
                                    Servers[i] = strArray[i];
                                    ClsIDs[i] = guidArray2[i];
                                }
                            }
                            strArray = null;
                            guidArray2 = null;
                            return;
                        }
                    }
                }
            }
            
        }
    }
}

