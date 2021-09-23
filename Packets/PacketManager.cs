namespace Packets
{
    public static class PacketManager
    {
        public static Packet GetPacket(byte[] data)
        {
            return new Packet(data).Opcode switch
            {
                Opcode.HandleMessage => new MessagePacket(data),
                Opcode.HandleConnect => new ConnectPacket(data),
                Opcode.HandleServerResponse => new ServerResponsePacket(data),
                Opcode.HandleBroadcast => new BroadcastPacket(data),
                _ => null
            };
        }
    }
}
