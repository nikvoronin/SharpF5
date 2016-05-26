using System.Net.Sockets;

namespace SharpF5.Helpers
{
    public class TcpHelper
    {
        public const int DEFAULT_PORT = 8000;
        
        public static NetworkStream Connect(string ipAddress)
        {
            TcpClient client = new TcpClient();
            client.ReceiveTimeout = 300;
            client.SendTimeout = 300;
            client.Connect(ipAddress, DEFAULT_PORT);

            return client.GetStream();
        }

        public static NetworkStream Connect(string ipAddress, int port, int timeout)
        {
            TcpClient client = new TcpClient();
            client.ReceiveTimeout = timeout;
            client.SendTimeout = timeout;
            client.Connect(ipAddress, port);

            return client.GetStream();
        }
    } // class
}
