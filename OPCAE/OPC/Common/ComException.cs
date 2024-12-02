using System;
using System.Runtime.InteropServices;
namespace OPCAE.OPC.Common
{

    public class ComException : ApplicationException
    {
        public int Error;

        public ComException(Exception e, string message) : base(message, e)
        {
            this.Error = 0;
            this.Error = Marshal.GetHRForException(e);
        }
    }
}

