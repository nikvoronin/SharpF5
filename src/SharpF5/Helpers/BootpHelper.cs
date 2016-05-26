using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace SharpF5.Helpers
{
    public class BootpHelper
    {
        public struct BootpResponse
        {
            public IPAddress IpAddress;
            public byte[] MacAddress;
            public string Type;
            public string Location;
        }

        public static IPAddress GetLocalIp()
        {
            string myHost = Dns.GetHostName();

            IPHostEntry myIPs = Dns.GetHostEntry(myHost);
            foreach (IPAddress myIP in myIPs.AddressList)
                if (myIP.GetAddressBytes()[0] != 127)
                    return myIP;

            return null;
        }

        public static void SetNetworkParameters(IPAddress ipAddress, byte[] mac, string location)
        {
            if (location == null)
                location = string.Empty;
            else
                if (location.Length > 31)
                    throw new IndexOutOfRangeException("Location must be smaller than 31 chars length");

            if (mac.Length != 6)
                throw new IndexOutOfRangeException("MAC must be 6 bytes length");

            byte[] local = ipAddress.GetAddressBytes();
            byte[] buffer = 
                {
                    local[0], local[1], local[2], local[3],
                    mac[0], mac[1], mac[2], mac[3], mac[4], mac[5],
                    0x32, 0,
                    0x08, 0x9e,
                    (byte)'I',(byte)'P',(byte)' ',(byte)'S',(byte)'c',(byte)'a',(byte)'n',(byte)'n',(byte)'e',(byte)'r',(byte)' ',(byte)'V',(byte)'2',(byte)'.',(byte)'1',0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                    0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0
                };
            ASCIIEncoding enc = new ASCIIEncoding();
            byte[] bLocation = enc.GetBytes(location);
            Array.Copy(bLocation, 0, buffer, 46, bLocation.Length);

            Socket sendsock =
                new Socket(
                    AddressFamily.InterNetwork,
                    SocketType.Dgram,
                    ProtocolType.Udp);
            sendsock.SetSocketOption(
                SocketOptionLevel.Socket,
                SocketOptionName.Broadcast,
                1);

            IPEndPoint sendep =
                new IPEndPoint(
                    IPAddress.Broadcast,
                    67);

            sendsock.SendTo(buffer, sendep);
        } // SetNetworkParameters()

        public static List<BootpResponse> BroadcastSearch(IPAddress localIp, int timeout)
        {
            byte[] local = localIp.GetAddressBytes();
            byte[] buffer = 
                {
                    local[0], local[1], local[2], local[3],
                    0,0,0,0,0,0,
                    0x29,0,
                    0x0e,0,
                    (byte)'I',(byte)'P',(byte)' ',(byte)'S',(byte)'c',(byte)'a',(byte)'n',(byte)'n',(byte)'e',(byte)'r',(byte)' ',(byte)'V',(byte)'2',(byte)'.',(byte)'1',0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                    (byte)'N',(byte)'v',(byte)'v',(byte)'.',(byte)'K',(byte)'e',(byte)'b',(byte)'.',(byte)'F',(byte)'5',0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0
                };

            Socket sendsock =
                new Socket(
                    AddressFamily.InterNetwork,
                    SocketType.Dgram,
                    ProtocolType.Udp);
            sendsock.SetSocketOption(
                SocketOptionLevel.Socket,
                SocketOptionName.Broadcast,
                1);

            IPEndPoint sendep =
                new IPEndPoint(
                    IPAddress.Broadcast,
                    67);

            sendsock.SendTimeout = timeout;
            sendsock.SendTo(buffer, sendep);

            // Listen to response
            IPEndPoint recep =
                new IPEndPoint(
                    IPAddress.Any,
                    68);
            Socket recsock =
                new Socket(
                    AddressFamily.InterNetwork,
                    SocketType.Dgram,
                    ProtocolType.Udp);
            recsock.Bind(recep);
            recsock.ReceiveTimeout = timeout;

            List<BootpResponse> list = new List<BootpResponse>();
            byte[] recbuf = new byte[300];
            EndPoint ip = recep;

            while (true)
            {
                try
                {
                    int reccnt = recsock.ReceiveFrom(recbuf, ref ip);
                    if (reccnt <= 0)
                        break;

                    BootpResponse response = new BootpResponse();
                    response.IpAddress = new IPAddress(new byte[] { recbuf[0], recbuf[1], recbuf[2], recbuf[3] });
                    response.MacAddress = new byte[6];
                    Array.Copy(recbuf, 4, response.MacAddress, 0, 6);
                        
                    ASCIIEncoding enc = new ASCIIEncoding();
                       
                    int zeroIdx = -1;
                    for (int i = 14; i < recbuf.Length; i++){ if (recbuf[i] == 0) { zeroIdx = i; break; }}

                    if (zeroIdx < 0)
                        continue;

                    byte[] bType = new byte[zeroIdx - 14];
                    Array.Copy(recbuf, 14, bType, 0, bType.Length);
                    response.Type = enc.GetString(bType);

                    zeroIdx = -1;
                    for (int i = 46; i < recbuf.Length; i++) { if (recbuf[i] == 0) { zeroIdx = i; break; } }
                    if (zeroIdx < 0)
                        continue;

                    bType = new byte[zeroIdx - 46];
                    Array.Copy(recbuf, 46, bType, 0, bType.Length);
                    response.Location = enc.GetString(bType);

                    list.Add(response);
                }
                catch (SocketException)
                {
                    break;
                }
            } // while(true)

            recsock.Shutdown(SocketShutdown.Both);

            return list;
        } // BroadcastSearch()
    } // class
}
