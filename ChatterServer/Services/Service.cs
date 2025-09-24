using SimplePackets;
using SimpleTcp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ChatterServer.Services
{
    internal class Service
    {
        private SimpleServer _server;
        private Task _updateTask;
        private Task _listenTask;

        void onNext(IChange change) => Model.Instance.AddChange(change);

        public Service()
        {
            onNext(new PortChange("8000"));
            onNext(new StatusChange("Idle"));

            Model.Instance.Changes.CollectionChanged += async (s, e) =>
            {
                foreach (var item in e.NewItems)
                {
                    switch (item)
                    {
                        case RunChange outputChange:
                            await Run();
                            break;
                        case StopChange outputChange:
                            await Stop();
                            break;
                    }
                }
            };
        }


        private async Task Run()
        {
            onNext(new StatusChange("Connecting..."));
            await SetupServer();
            _server.Open();
            _listenTask = Task.Run(() => _server.Start());
            _updateTask = Task.Run(() => Update());
            onNext(new IsRunningChange(true));
        }

        private async Task SetupServer()
        {
            onNext(new StatusChange("Validating socket..."));

            if (!int.TryParse(Model.Instance.String<PortChange>(), out var socketPort))
            {
                DisplayError("Port value is not valid.");
                return;
            }

            onNext(new StatusChange("Obtaining IP..."));
            await Task.Run(() => GetExternalIp());
            onNext(new StatusChange("Setting up server..."));
            _server = new SimpleServer(IPAddress.Any, socketPort);
            onNext(new StatusChange("Setting up events..."));
            _server.OnConnectionAccepted += Server_OnConnectionAccepted;
            _server.OnConnectionRemoved += Server_OnConnectionRemoved;
            _server.OnPacketSent += Server_OnPacketSent;
            _server.OnPersonalPacketSent += Server_OnPersonalPacketSent;
            _server.OnPersonalPacketReceived += Server_OnPersonalPacketReceived;
            _server.OnPacketReceived += Server_OnPacketReceived;
        }

        private void Update()
        {
            while (Model.Instance.Bool<IsRunningChange>())
            {
                Thread.Sleep(5);
                if (!_server.IsRunning)
                {
                    Task.Run(() => Stop());
                    return;
                }

                onNext(new ClientsConnectedChange(_server.Connections.Count));
                onNext(new StatusChange("Running"));
            }
        }

        private async Task Stop()
        {
            onNext(new ExternalAddressChange(string.Empty));
            onNext(new IsRunningChange(false));
            onNext(new ClientsConnectedChange(0));
            _server.Close();

            await _listenTask;
            await _updateTask;
            onNext(new StatusChange("Stopped"));
        }

        private void GetExternalIp()
        {
            try
            {
                string externalIP;
                //externalIP = (new WebClient()).DownloadString("http://checkip.dyndns.org/");
                //externalIP = (new Regex(@"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}"))
                //             .Matches(externalIP)[0].ToString();
                externalIP = "127.0.0.1";
                //ExternalAddress = externalIP;
                onNext(new ExternalAddressChange(externalIP));
            }
            catch
            {
                onNext(new ExternalAddressChange("Error receiving IP address."));
            }
        }

        private void DisplayError(string message)
        {
            MessageBox.Show(message, "Woah there!", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void Server_OnPacketSent(object sender, PacketEvents e)
        {

        }

        private void Server_OnPacketReceived(object sender, PacketEvents e)
        {

        }

        private void Server_OnPersonalPacketSent(object sender, PersonalPacketEvents e)
        {
            WriteOutput("Personal Packet Sent");
        }
        private void Server_OnPersonalPacketReceived(object sender, PersonalPacketEvents e)
        {
            if (e.Packet.Package is UserConnectionPacket ucp)
            {
                Task.Run(() => _server.SendObjectToClients(convert(ucp))).Wait();
                Thread.Sleep(500);
                Task.Run(() => _server.SendObjectToClients(new ChatPacket
                {
                    Username = "Server",
                    Message = "A new user has joined the chat",
                    UserColor = Colors.Purple.ToString(),

                })).Wait();
            }
            WriteOutput("Personal Packet Received");

            UserConnectionPacket convert(UserConnectionPacket ucp)
            {
                var users = Model.Instance.Dictionary<UserNameAddChange>();
                if (users.Keys.Contains(ucp.UserGuid))
                {
                    users.Remove(ucp.UserGuid);
                    onNext(new UserNameRemoveChange(ucp.UserGuid, ucp.Username));
                }
                else
                {
                    users.Add(ucp.UserGuid, ucp.Username);
                    onNext(new UserNameAddChange(ucp.UserGuid, ucp.Username));
                }
                ucp.Users = [.. users.Values];
                return ucp;
            }
        }

        private void Server_OnConnectionAccepted(object sender, PacketEvents e)
        {
            WriteOutput("Client Connected: " + e.Sender.Socket.RemoteEndPoint.ToString());
        }

        private void Server_OnConnectionRemoved(object sender, PacketEvents e)
        {
            Dictionary<string, string> Usernames = Model.Instance.Dictionary<UserNameAddChange>();
            if (!Usernames.ContainsKey(e.Sender.ClientId.ToString()))
                return;

            var notification = new ChatPacket
            {
                Username = "Server",
                Message = "A user has left the chat",
                UserColor = Colors.Purple.ToString()
            };

            var userGuid = e.Sender.ClientId.ToString();
            var userName = Usernames[e.Sender.ClientId.ToString()];
            if (Usernames.Keys.Contains(userGuid))
                Usernames.Remove(userGuid);

            var userPacket = new UserConnectionPacket
            {
                UserGuid = userGuid,
                Username = userName,
                IsJoining = false,
                Users = [.. Usernames.Values]
            };

            if (_server.Connections.Count != 0)
            {
                Task.Run(() => _server.SendObjectToClients(userPacket)).Wait();
                Task.Run(() => _server.SendObjectToClients(notification)).Wait();
            }
            WriteOutput("Client Disconnected: " + e.Sender.Socket.RemoteEndPoint.ToString());
        }

        private void WriteOutput(string message)
        {
            onNext(new OutputChange(message));
        }
    }
}
