using System;

namespace OPCAE.OPCAE.NET
{
    public class ShutdownRequestEventArgs : EventArgs
    {
        public string shutdownReason;

        public ShutdownRequestEventArgs(string shutdownReasonp)
        {
            this.shutdownReason = shutdownReasonp;
        }
    }
}

