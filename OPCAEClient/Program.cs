using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OPCAEClient
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //var aa = WindowsIdentity.GetCurrent();
            string userName = WindowsIdentity.GetCurrent().Name;
            Application.Run(new Form1());
            //if (userName.Contains("opcuser"))
            //{
            //    Application.Run(new Form1());
            //}
            //else
            //{
            //    ProcessStartInfo startInfo = new ProcessStartInfo();
            //    startInfo.FileName = "OPCAEClient.exe";
            //    startInfo.UserName = "opcuser";
            //    SecureString password = new SecureString();
            //    password.AppendChar('p');
            //    password.AppendChar('a');
            //    password.AppendChar('s');
            //    password.AppendChar('s');
            //    password.AppendChar('w');
            //    password.AppendChar('o');
            //    password.AppendChar('r');
            //    password.AppendChar('d');
            //    startInfo.Password = password;
            //    startInfo.LoadUserProfile = true;
            //    startInfo.UseShellExecute = false;
            //    try
            //    {
            //        Process.Start(startInfo);
            //    }
            //    catch(Exception e)
            //    {
            //        return;
            //    }
            //}
        }
    }
}
