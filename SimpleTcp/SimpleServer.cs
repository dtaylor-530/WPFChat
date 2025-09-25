using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace SimpleTcp
{
    public class SimpleServer
    {
        private Task _receivingTask;

        public const string OnConnectionAccepted = nameof(OnConnectionAccepted);
        public const string OnConnectionRejected = nameof(OnConnectionRejected);
        public const string OnConnectionRemoved = nameof(OnConnectionRemoved);
        public const string OnPacketReceived = nameof(OnPacketReceived);
        public const string OnPersonalPacketReceived = nameof(OnPersonalPacketReceived);
        public const string OnPersonalPacketSent = nameof(OnPersonalPacketSent);
        public const string OnPacketSent = nameof(OnPacketSent);

        public SimpleServer(IPAddress address, int port)
        {
            Address = address;
            Port = port;

            EndPoint = new IPEndPoint(address, port);

            Socket = new (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            {
                ReceiveTimeout = 5000
            };
        }

        public IPAddress Address { get; }
        public int Port { get; }
        public IPEndPoint EndPoint { get; }
        public Socket Socket { get; }
        public bool IsRunning { get; set; }
        public ObservableCollection<SimpleClient> Connections { get; } = [];
        public ObservableCollection<PacketEvent> PacketEvents { get; } = [];

        public bool Open()
        {
            Socket.Bind(EndPoint);
            Socket.Listen(10);
            return true;
        }

        public async Task<bool> Start()
        {
            _receivingTask = Task.Run(() => MonitorStreams());
            IsRunning = true;
            await Listen();
            await _receivingTask;
            Socket.Close();
            return true;
        }

        public bool Close()
        {
            IsRunning = false;
            Connections.Clear();
            return true;
        }

        public async Task<bool> Listen()
        {
            while (IsRunning)
            {
                if (Socket.Poll(100000, SelectMode.SelectRead))
                {
                    var newConnection = Socket.Accept();
                    if (newConnection != null)
                    {
                        Guid newGuid = Guid.NewGuid();
                        var client = new SimpleClient
                        {
                            Socket = newConnection,
                            EndPoint = ((IPEndPoint)newConnection.LocalEndPoint),
                            ClientId = newGuid
                        };
                        await newConnection.SendMessage(newGuid.ToString());
                        Connections.Add(client);
                        addEvent(OnConnectionAccepted, client, null, String.Empty);
                    }
                    else
                    {
                        addEvent(OnConnectionRejected, null, null, String.Empty);
                    }
                }
            }
            return true;
        }

        private void MonitorStreams()
        {
            while (IsRunning)
            {
                foreach(var client in Connections.ToArray())
                {
                    if (!client.Socket.IsConnected())
                    {
                        Connections.Remove(client);
                        addEvent(OnConnectionRemoved, client, null, string.Empty);                       
                        continue;
                    }

                    if(client.Socket.Available != 0)
                    {
                        var readObject = Helper.TryReceiveObject(client.Socket);
                        addEvent(OnPacketReceived, client, null, readObject);

                        if(readObject is PingPacket ping)
                        {
                            client.Socket.SendObject(ping).Wait();
                            continue;
                        }

                        if(readObject is PersonalPacket pp)
                        {
                            var destination = Connections.FirstOrDefault(c => c.ClientId.ToString() == pp.GuidId);
                            addEvent(OnPersonalPacketReceived, client, destination, pp);

                            if(destination != null)
                            {
                                destination.Socket.SendObject(pp).Wait();
                               addEvent(OnPersonalPacketSent, client, destination, pp);
                            }
                        }
                        else
                        {
                            foreach (var c in Connections.ToList())
                            {
                                c.Socket.SendObject(readObject).Wait();
                               addEvent(OnPacketSent, client, c, readObject);
                            }
                        }
                    }
                }
            }
        }

        public void SendObjectToClients(object package)
        {
            foreach (var c in Connections.ToList())
            {
                c.Socket.SendObject(package).Wait();
                addEvent(OnPacketSent, c, c, package);
            }
        }

        private void addEvent(string name, SimpleClient sender, SimpleClient receiver, object package)
        {
            PacketEvents.Add(new PacketEvent
            {
                Name = name,
                Sender = sender.ClientId,
                Receiver = receiver?.ClientId,
                Packet = package
            });
        }

    }
}
