using System;

namespace OPCAE.OPC
{
    public class OPCException : Exception
    {
        private string message;

        public OPCException(int rtc)
        {
            base.HResult = rtc;
        }

        public OPCException(int rtc, string msg)
        {
            if (msg.IndexOf("{hr}") >= 0)
            {
                msg = msg.Replace("{hr}", "0x" + rtc.ToString("X") + " - " + ErrorDescriptions.GetErrorDescription(rtc));
            }
            base.HResult = rtc;
            this.message = msg;
        }

        public int Result =>
            base.HResult;

        public override string Message =>
            this.message;
    }
}

