using System.Text;

namespace Packets
{
    public class ConnectPacket : Packet
    {
        internal ConnectPacket(byte[] data) : base(data)
        {
            Username = Encoding.ASCII.GetString(Reader.ReadBytes(16)).Trim('\0');
        }

        public string Username { get; }
    }
}
