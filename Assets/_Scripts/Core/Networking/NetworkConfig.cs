
using System.Net;
using System.Net.Sockets;

namespace Core
{
    public static class NetworkConfig
    {
        public static UdpClient ActiveClient { get; set; }

        public static int RemotePort { get; set; }

        public static IPAddress IPAddress { get; set; } = IPAddress.None;

        public static int LocalPlayerId { get; set; }

        public static int MinArtificialDelay { get; set; }
        
        public static int MaxArtificialDelay { get; set; }
        
        public static int PacketLossPercentage { get; set; }
    }
}