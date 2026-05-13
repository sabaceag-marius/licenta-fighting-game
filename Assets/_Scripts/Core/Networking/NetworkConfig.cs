
namespace Core
{
    public static class NetworkConfig
    {
        public static int LocalPort { get; set; }
        
        public static int RemotePort { get; set; }

        public static string IPAddress { get; set; } = "127.0.0.1";
    }
}