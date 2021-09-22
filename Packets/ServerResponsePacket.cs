using System.Text;

namespace Packets
{
    public class ServerResponsePacket : Packet
    {
        internal ServerResponsePacket(byte[] data) : base(data)
        {
            Length = Reader.ReadUInt16();
            Success = Reader.ReadBoolean();
            OnlineCount = Reader.ReadUInt16();
            ServerName = Encoding.ASCII.GetString(Reader.ReadBytes(32)).Trim('\0');
            Message = Encoding.ASCII.GetString(Reader.ReadBytes(Length)).Trim('\0');
        }

        public ushort Length { get; }
        public bool Success { get; }
        public ushort OnlineCount { get; }
        public string ServerName { get; }
        public string Message { get; }
    }
}
