using System.Text;

namespace Packets
{
    public class MessagePacket : Packet
    {
        internal MessagePacket(byte[] data) : base(data)
        {
            Length = Reader.ReadUInt16();
            Color = Reader.ReadByte();
            Username = Encoding.ASCII.GetString(Reader.ReadBytes(16)).Trim('\0');
            Message = Encoding.ASCII.GetString(Reader.ReadBytes(Length)).Trim('\0');
        }

        public ushort Length { get; }
        public byte Color { get; }
        public string Username { get; }
        public string Message { get; }
    }
}
