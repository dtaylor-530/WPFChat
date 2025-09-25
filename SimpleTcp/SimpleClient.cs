using System;
using System.Net;
using System.Net.Sockets;

namespace SimpleTcp
{
    public class SimpleClient
    {
        public Guid ClientId { get; set; }
        public Socket Socket { get; set; }
        public IPEndPoint EndPoint { get; set; }
        public IPAddress Address { get; }
        //public bool IsConnected { get; private set; }

        public bool IsGuidAssigned { get; set; }

        public int ReceiveBufferSize
        {
            get { return Socket.ReceiveBufferSize; }
            set { Socket.ReceiveBufferSize = value; }
        }

        public int SendBufferSize
        {
            get { return Socket.SendBufferSize; }
            set { Socket.SendBufferSize = value; }
        }

        public SimpleClient(string address, int port)
        {
            if (!IPAddress.TryParse(address, out IPAddress ipAddress))
                ipAddress = Dns.GetHostAddresses(address)[0];

            Address = ipAddress;
            EndPoint = new IPEndPoint(ipAddress, port);
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ReceiveBufferSize = 8000;
            SendBufferSize = 8000;
        }

        public SimpleClient()
        {
        }

    
    }
}
