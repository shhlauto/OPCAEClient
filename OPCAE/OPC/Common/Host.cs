namespace OPCAE.OPC.Common
{
    public class Host
    {
        public string HostName;
        public string UserName;
        public string Password;
        public string Domain;

        public Host()
        {
            this.HostName = null;
            this.UserName = null;
            this.Password = null;
            this.Domain = null;
        }

        public Host(string hostName)
        {
            this.HostName = null;
            this.UserName = null;
            this.Password = null;
            this.Domain = null;
            this.HostName = hostName;
        }
    }
}

