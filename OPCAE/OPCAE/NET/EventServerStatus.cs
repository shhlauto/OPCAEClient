namespace OPCAE.OPCAE.NET
{
    using OPCAE;
    using System;

    public class EventServerStatus
    {
        public DateTime StartTime;
        public DateTime CurrentTime;
        public DateTime LastUpdateTime;
        public OPCEventServerState ServerState;
        public short MajorVersion;
        public short MinorVersion;
        public short BuildNumber;
        public string VendorInfo;
    }
}

