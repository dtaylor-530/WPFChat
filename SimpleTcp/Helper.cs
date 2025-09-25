using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace SimpleTcp
{
    public static class Helper
    {

        public static async Task<Guid?> ConnectTo(this Socket Socket, IPEndPoint EndPoint)
        {
            var result = await Task.Run(() => Helper.TryConnect(Socket, EndPoint));

            if (result is not Exception exception)
            {
                if (Helper.RecieveGuid(Socket) is string str && Guid.TryParse(str, out Guid guid))
                {
                    return guid;           
                }
            }

            return default;
        }

        public static async Task<IOException> SendMessage(this Socket Socket, string message) => await Task.Run(() => Helper.TrySendMessage(Socket, message));

        public static async Task<IOException> SendObject(this Socket Socket, object obj) => await Task.Run(() => Helper.TrySendObject(Socket, obj));

        public static async Task<object> ReceiveObject(this Socket Socket) => await Task.Run(() => Helper.TryReceiveObject(Socket));

        public static async Task<Exception> PingConnection(this Socket Socket)
        {
            try
            {
                return await Task.Run(() => SendObject(Socket, new PingPacket()));
            }
            catch (ObjectDisposedException e)
            {
                return e;
            }
        }

        public static void Disconnect(this Socket Socket)
        {
            Socket.Close();
        }


        public static object TryReceiveObject(this Socket socket)
        {
            if (socket.Available == 0)
                return null;

            byte[] data = new byte[socket.ReceiveBufferSize];

            try
            {
                using (Stream s = new NetworkStream(socket))
                {
                    s.Read(data, 0, data.Length);
                    return new BinaryFormatter().Deserialize(new MemoryStream(data)
                    {
                        Position = 0
                    });
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("TryRecieveObject " + e.Message);
                return null;
            }
        }

        public static IOException TrySendObject(this Socket socket, object obj)
        {
            try
            {
                if(obj is string x)
                {

                }
                using (Stream s = new NetworkStream(socket))
                {
                    var memory = new MemoryStream();
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(memory, obj);
                    var newObj = memory.ToArray();

                    memory.Position = 0;
                    s.Write(newObj, 0, newObj.Length);
                    return null;
                }   
            }
            catch (IOException e)
            {

                return e;
            }
        }

        public static IOException TrySendMessage(this Socket socket, string message)
        {
            try
            {
                using (Stream s = new NetworkStream(socket))
                {
                    new StreamWriter(s)
                    {
                        AutoFlush = true
                    }.WriteLine(message);
                    return null;
                }
            }
            catch (IOException e)
            {

                return e;
            }
        }

        public static Exception TryConnect(this Socket socket, IPEndPoint endPoint)
        {
            try
            {
                socket.Connect(endPoint);
                return null;
            }
            catch(Exception e)
            {
                return e;
            }
        }

        public static object RecieveGuid(this Socket socket)
        {
            try
            {
                using (Stream s = new NetworkStream(socket))
                {
                    var reader = new StreamReader(s);
                    s.ReadTimeout = 5000;

                    return reader.ReadLine();
                }
            }
            catch (IOException e)
            {
                return e;
            }
        }


        //https://stackoverflow.com/questions/2661764/how-to-check-if-a-socket-is-connected-disconnected-in-c
        public static bool IsConnected(this Socket socket)
        {
            try
            {
                bool part1 = socket.Poll(5000, SelectMode.SelectRead);
                bool part2 = (socket.Available == 0);
                if (part1 && part2)
                    return false;
                else
                    return true;
            }
            catch (ObjectDisposedException)
            {
                return false;
            }
        }




    }
}
