namespace LawPortal.Web.Services
{
    public class NetworkShareSettings
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Domain { get; set; }
        // Use IP address (e.g. \\192.168.1.10\test) to avoid SMB session conflicts
        // when the app pool already has a cached session to the same server by hostname.
        public string BaseSharePath { get; set; }
    }
}
