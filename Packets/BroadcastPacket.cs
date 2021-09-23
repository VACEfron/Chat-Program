using System.Text;

namespace Packets
{
    public class BroadcastPacket : Packet
    {
        internal BroadcastPacket(byte[] data) : base(data)
        {
            ushort length = Reader.ReadUInt16();

            Message = Encoding.ASCII.GetString(Reader.ReadBytes(length)).Trim('\0');
        }

        public string Message { get; }
    }
}
