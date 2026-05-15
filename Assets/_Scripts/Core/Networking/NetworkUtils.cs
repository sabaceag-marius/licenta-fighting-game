using System;
using System.Net;
using System.Net.Sockets;
using Data;

namespace Core.Networking
{
    public static class NetworkUtils
    {
        // Number of inputs sent in a packet
        public const int REDUNDANCY_COUNT = 30;
        
        // The exact size of our packet in bytes
        public const int PACKET_SIZE = 4 + REDUNDANCY_COUNT * 6;

        /// <summary>
        /// Writes the packet data directly into a pre-allocated byte array.
        /// </summary>
        public static void Serialize(NetworkPacket packet, byte[] buffer)
        {
            if (buffer.Length < PACKET_SIZE)
                throw new ArgumentException("Buffer is too small!");
                
            buffer[0] = packet.PlayerId;
            buffer[1] = (byte)(packet.LatestExecutionFrame & 0xFF);
            buffer[2] = (byte)((packet.LatestExecutionFrame >> 8) & 0xFF);
            buffer[3] = (byte)packet.RawAdvantage;

            int offset = 4;

            for (int i = 0; i < REDUNDANCY_COUNT; i++)
            {
                RawInput input = packet.Inputs[i];
                buffer[offset] = (byte)(input.FrameId & 0xFF);
                buffer[offset + 1] = (byte)((input.FrameId >> 8) & 0xFF);
                buffer[offset + 2] = (byte)input.LeftStickX;
                buffer[offset + 3] = (byte)input.LeftStickY;
                buffer[offset + 4] = input.RightStick;
                buffer[offset + 5] = input.Buttons;

                offset += 6;
            }
        }

        /// <summary>
        /// Reads the packet data directly from a byte array.
        /// </summary>
        public static NetworkPacket Deserialize(byte[] buffer)
        {
            if (buffer.Length < PACKET_SIZE)
                throw new ArgumentException("Buffer is too small!");

            NetworkPacket packet = new NetworkPacket();
            packet.PlayerId = buffer[0];
            packet.LatestExecutionFrame = (ushort)(buffer[1] | (buffer[2] << 8));
            packet.RawAdvantage = (sbyte)buffer[3];

            packet.Inputs = new RawInput[REDUNDANCY_COUNT];

            int offset = 4;
            for (int i = 0; i < REDUNDANCY_COUNT; i++)
            {
                packet.Inputs[i] = new RawInput
                {
                    FrameId = (ushort)(buffer[offset] | (buffer[offset + 1] << 8)),
                    LeftStickX = (sbyte)buffer[offset + 2],
                    LeftStickY = (sbyte)buffer[offset + 3],
                    RightStick = buffer[offset + 4],
                    Buttons = buffer[offset + 5]
                };
                offset += 6;
            }
            
            // Reconstruct the ushort by shifting the second byte left by 8 bits and combining them
            // packet.Input.FrameId = (ushort)(buffer[0] | (buffer[1] << 8));
            
            // // Reconstruct the remaining bytes
            // packet.Input.LeftStickX = (sbyte)buffer[2];
            // packet.Input.LeftStickY = (sbyte)buffer[3];
            // packet.Input.RightStick = buffer[4];
            // packet.Input.Buttons = buffer[5];

            return packet;
        }

        public static string FindLocalIP()
        {
            foreach(IPAddress ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if(ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return null;
        }
    }
}