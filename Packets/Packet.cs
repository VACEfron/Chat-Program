using System.IO;

namespace Packets
{
    public class Packet
    {
        internal Packet(byte[] data)
        {
            Reader = new BinaryReader(new MemoryStream(data));
            Opcode = (Opcode)Reader.ReadByte();
            RawBytes = data;
        }

        protected BinaryReader Reader { get; }
        public Opcode Opcode { get; }
        public byte[] RawBytes { get; }
    }
}