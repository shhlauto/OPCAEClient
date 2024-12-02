
using OPCAE.OPC;
using OPCAE.OPCAE.Contract;
using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security;
using System.Security.Permissions;
using System.Windows.Forms;

namespace OPCAE.OPCAE.NET
{
    [ComVisible(true), SuppressUnmanagedCodeSecurity, ReflectionPermission(SecurityAction.Assert, Unrestricted = true), SecurityPermission(SecurityAction.Assert, UnmanagedCode = true)]
    public class AreaBrowser
    {
        private Guid riidAreaBrowse = new Guid("{65168857-5783-11D1-84A0-00608CB8A7E9}");
        private IOPCEventAreaBrowser ifAreaBrowser;
        private OpcEventServer Srv;

        private int browseAreas(TreeNode branch, string filterCriteria)
        {
            string[] strArray;
            int hresultcode = this.BrowseOPCAreas(OPCAEBrowseType.OPC_AREA, filterCriteria, out strArray);
            if (HRESULTS.Failed(hresultcode))
            {
                if (OpcEventServer.myErrorsAsExecptions)
                {
                    throw new OPCException(hresultcode, ErrorDescriptions.GetErrorDescription(hresultcode));
                }
                return hresultcode;
            }
            if ((strArray == null) || (strArray.Length == 0))
            {
                return 1;
            }
            foreach (string str in strArray)
            {
                TreeNode[] children = new TreeNode[0];
                TreeNode node = new TreeNode(str, children);
                string str2;
                hresultcode = this.GetQualifiedAreaName(str, out str2);
                if (HRESULTS.Failed(hresultcode))
                {
                    if (OpcEventServer.myErrorsAsExecptions)
                    {
                        throw new OPCException(hresultcode, ErrorDescriptions.GetErrorDescription(hresultcode));
                    }
                    return hresultcode;
                }
                node.Tag = str2;
                branch.Nodes.Add(node);
                hresultcode = this.ChangeBrowsePosition(OPCAEBrowseDirection.OPCAE_BROWSE_DOWN, str);
                if (HRESULTS.Failed(hresultcode))
                {
                    if (OpcEventServer.myErrorsAsExecptions)
                    {
                        throw new OPCException(hresultcode, ErrorDescriptions.GetErrorDescription(hresultcode));
                    }
                    return hresultcode;
                }
                hresultcode = this.browseAreas(node, filterCriteria);
                if (HRESULTS.Failed(hresultcode))
                {
                    if (OpcEventServer.myErrorsAsExecptions)
                    {
                        throw new OPCException(hresultcode, ErrorDescriptions.GetErrorDescription(hresultcode));
                    }
                    return hresultcode;
                }
                hresultcode = this.ChangeBrowsePosition(OPCAEBrowseDirection.OPCAE_BROWSE_UP, "");
                if (HRESULTS.Failed(hresultcode))
                {
                    if (OpcEventServer.myErrorsAsExecptions)
                    {
                        throw new OPCException(hresultcode, ErrorDescriptions.GetErrorDescription(hresultcode));
                    }
                    return hresultcode;
                }
            }
            return 0;
        }

        private int BrowseOPCAreas(OPCAEBrowseType BrowseFilterType, string FilterCriteria, out UCOMIEnumString EnumString)
        {
            int hresultcode = -2147467259;
            EnumString = null;
            object ppIEnumString = EnumString;
            if (OpcEventServer.myErrorsAsExecptions)
            {
                hresultcode = this.ifAreaBrowser.BrowseOPCAreas(BrowseFilterType, FilterCriteria, out ppIEnumString);
            }
            else
            {
                try
                {
                    hresultcode = this.ifAreaBrowser.BrowseOPCAreas(BrowseFilterType, FilterCriteria, out ppIEnumString);
                }
                catch (Exception exception)
                {
                    string message = exception.Message;
                }
            }
            EnumString = (UCOMIEnumString)ppIEnumString;
            ppIEnumString = null;
            if (OpcEventServer.myErrorsAsExecptions && HRESULTS.Failed(hresultcode))
            {
                throw new OPCException(hresultcode, ErrorDescriptions.GetErrorDescription(hresultcode));
            }
            return hresultcode;
        }

        public int BrowseOPCAreas(OPCAEBrowseType BrowseFilterType, string FilterCriteria, out string[] areas)
        {
            areas = null;
            UCOMIEnumString str;
            int hresultcode = this.BrowseOPCAreas(BrowseFilterType, FilterCriteria, out str);
            if (HRESULTS.Failed(hresultcode))
            {
                if (OpcEventServer.myErrorsAsExecptions)
                {
                    throw new OPCException(hresultcode, ErrorDescriptions.GetErrorDescription(hresultcode));
                }
                return hresultcode;
            }
            if (str == null)
            {
                return 1;
            }
            string[] rgelt = new string[1];
            int celt = 0;
            int num3 = 0;
            while (str.Next(1, rgelt, out num3) == 0)
            {
                celt++;
            }
            areas = new string[celt];
            str.Reset();
            str.Next(celt, areas, out num3);
            return 0;
        }

        private int browseSources(TreeNode branch, string filterCriteria)
        {
            string[] strArray;
            int hresultcode = this.BrowseOPCAreas(OPCAEBrowseType.OPC_SOURCE, filterCriteria, out strArray);
            if (HRESULTS.Failed(hresultcode))
            {
                if (OpcEventServer.myErrorsAsExecptions)
                {
                    throw new OPCException(hresultcode, ErrorDescriptions.GetErrorDescription(hresultcode));
                }
                return hresultcode;
            }
            if ((strArray != null) && (strArray.Length > 0))
            {
                foreach (string str in strArray)
                {
                    TreeNode node = new TreeNode(str);
                    string str2;
                    hresultcode = this.GetQualifiedSourceName(str, out str2);
                    if (HRESULTS.Failed(hresultcode))
                    {
                        if (OpcEventServer.myErrorsAsExecptions)
                        {
                            throw new OPCException(hresultcode, ErrorDescriptions.GetErrorDescription(hresultcode));
                        }
                        return hresultcode;
                    }
                    node.Tag = str2;
                    node.ImageIndex = 0x3e7;
                    branch.Nodes.Add(node);
                }
            }
            string[] strArray2;
            hresultcode = this.BrowseOPCAreas(OPCAEBrowseType.OPC_AREA, filterCriteria, out strArray2);
            if (HRESULTS.Failed(hresultcode))
            {
                if (OpcEventServer.myErrorsAsExecptions)
                {
                    throw new OPCException(hresultcode, ErrorDescriptions.GetErrorDescription(hresultcode));
                }
                return hresultcode;
            }
            if (((strArray2 == null) || (strArray2.Length == 0)) && ((strArray == null) || (strArray.Length == 0)))
            {
                return 1;
            }
            foreach (string str3 in strArray2)
            {
                TreeNode[] children = new TreeNode[0];
                TreeNode node = new TreeNode(str3, children);
                string str4;
                hresultcode = this.GetQualifiedAreaName(str3, out str4);
                if (HRESULTS.Failed(hresultcode))
                {
                    if (OpcEventServer.myErrorsAsExecptions)
                    {
                        throw new OPCException(hresultcode, ErrorDescriptions.GetErrorDescription(hresultcode));
                    }
                    return hresultcode;
                }
                node.Tag = str4;
                branch.Nodes.Add(node);
                hresultcode = this.ChangeBrowsePosition(OPCAEBrowseDirection.OPCAE_BROWSE_DOWN, str3);
                if (HRESULTS.Failed(hresultcode))
                {
                    if (OpcEventServer.myErrorsAsExecptions)
                    {
                        throw new OPCException(hresultcode, ErrorDescriptions.GetErrorDescription(hresultcode));
                    }
                    return hresultcode;
                }
                hresultcode = this.browseSources(node, filterCriteria);
                if (HRESULTS.Failed(hresultcode))
                {
                    if (OpcEventServer.myErrorsAsExecptions)
                    {
                        throw new OPCException(hresultcode, ErrorDescriptions.GetErrorDescription(hresultcode));
                    }
                    return hresultcode;
                }
                hresultcode = this.ChangeBrowsePosition(OPCAEBrowseDirection.OPCAE_BROWSE_UP, "");
                if (HRESULTS.Failed(hresultcode))
                {
                    if (OpcEventServer.myErrorsAsExecptions)
                    {
                        throw new OPCException(hresultcode, ErrorDescriptions.GetErrorDescription(hresultcode));
                    }
                    return hresultcode;
                }
            }
            return 0;
        }

        public int ChangeBrowsePosition(OPCAEBrowseDirection BrowseDirection, string String)
        {
            int hresultcode = this.ifAreaBrowser.ChangeBrowsePosition(BrowseDirection, String);
            if (OpcEventServer.myErrorsAsExecptions && HRESULTS.Failed(hresultcode))
            {
                throw new OPCException(hresultcode, ErrorDescriptions.GetErrorDescription(hresultcode));
            }
            return hresultcode;
        }

        public int Create(OpcEventServer srv)
        {
            this.Srv = srv;
            object obj2;
            int hresultcode = srv.ifAEServer.CreateAreaBrowser(ref this.riidAreaBrowse, out obj2);
            this.ifAreaBrowser = (IOPCEventAreaBrowser)obj2;
            obj2 = null;
            if (OpcEventServer.myErrorsAsExecptions && HRESULTS.Failed(hresultcode))
            {
                throw new OPCException(hresultcode, ErrorDescriptions.GetErrorDescription(hresultcode));
            }
            return hresultcode;
        }

        public void Dispose()
        {
            if (this.ifAreaBrowser != null)
            {
                Marshal.ReleaseComObject(this.ifAreaBrowser);
            }
        }

        ~AreaBrowser()
        {
            this.Dispose();
        }

        public int GetQualifiedAreaName(string AreaName, out string QualifiedAreaName)
        {
            int qualifiedAreaName = this.ifAreaBrowser.GetQualifiedAreaName(AreaName, out QualifiedAreaName);
            if (OpcEventServer.myErrorsAsExecptions && HRESULTS.Failed(qualifiedAreaName))
            {
                throw new OPCException(qualifiedAreaName, ErrorDescriptions.GetErrorDescription(qualifiedAreaName));
            }
            return qualifiedAreaName;
        }

        public int GetQualifiedSourceName(string SourceName, out string QualifiedSourceName)
        {
            int qualifiedSourceName = this.ifAreaBrowser.GetQualifiedSourceName(SourceName, out QualifiedSourceName);
            if (OpcEventServer.myErrorsAsExecptions && HRESULTS.Failed(qualifiedSourceName))
            {
                throw new OPCException(qualifiedSourceName, ErrorDescriptions.GetErrorDescription(qualifiedSourceName));
            }
            return qualifiedSourceName;
        }

        public int GetTree(OPCAEBrowseType bType, string filterCriteria, out TreeNode[] tree)
        {
            tree = null;
            int hresultcode = this.ChangeBrowsePosition(OPCAEBrowseDirection.OPCAE_BROWSE_TO, "");
            if (HRESULTS.Failed(hresultcode))
            {
                if (OpcEventServer.myErrorsAsExecptions)
                {
                    throw new OPCException(hresultcode, ErrorDescriptions.GetErrorDescription(hresultcode));
                }
                return hresultcode;
            }
            TreeNode branch = new TreeNode("root");
            if (bType == OPCAEBrowseType.OPC_AREA)
            {
                hresultcode = this.browseAreas(branch, filterCriteria);
            }
            else
            {
                hresultcode = this.browseSources(branch, filterCriteria);
            }
            if (HRESULTS.Failed(hresultcode))
            {
                if (OpcEventServer.myErrorsAsExecptions)
                {
                    throw new OPCException(hresultcode, ErrorDescriptions.GetErrorDescription(hresultcode));
                }
                return hresultcode;
            }
            tree = new TreeNode[branch.Nodes.Count];
            int num2 = 0;
            foreach (TreeNode node2 in branch.Nodes)
            {
                tree[num2++] = node2;
            }
            return 0;
        }
    }
}

