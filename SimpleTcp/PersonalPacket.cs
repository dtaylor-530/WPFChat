using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleTcp
{
    [Serializable]
    public class PersonalPacket
    {
        public string GuidId { get; set; }
        public object Package { get; set; }
    }

    [Serializable]
    public class PingPacket
    {
        public string GuidId { get; set; }
    }

    public class PacketEvent : EventArgs
    {
        public object Sender;
        public object Receiver;
        public object Packet;

        public string Name { get; internal set; }
    }

    public class PersonalPacketEvents : EventArgs
    {
        public object Sender;
        public object Receiver;
        public PersonalPacket Packet;
    }
}
