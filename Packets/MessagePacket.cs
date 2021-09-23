using System.Text;

namespace Packets
{
    public class MessagePacket : Packet
    {
        internal MessagePacket(byte[] data) : base(data)
        {
            ushort length = Reader.ReadUInt16();

            Color = Reader.ReadByte();
            Username = Encoding.ASCII.GetString(Reader.ReadBytes(16)).Trim('\0');
            Message = Encoding.ASCII.GetString(Reader.ReadBytes(length)).Trim('\0');
        }

        public byte Color { get; }
        public string Username { get; }
        public string Message { get; }
    }
}
