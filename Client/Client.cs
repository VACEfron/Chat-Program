using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Packets;

namespace Client
{
    public class Client
    {
        private string _ip;
        private int _port;
        private readonly byte _color;

        private readonly TcpClient _client;
        private NetworkStream _stream;

        private string _username;

        public Client()
        {
            _client = new TcpClient();

            var colors = new byte[] { 9, 11, 1, 3, 2, 4, 6, 10, 13, 12, 14 };
            _color = colors[new Random().Next(colors.Length)];
        }

        public async Task RunClientAsync()
        {
            AskIpAdressAndPort();
            AskUsername();

            await _client.ConnectAsync(_ip, _port);
            _stream = _client.GetStream();

            await SendConnectAsync();

            _ = Task.Run(() => ReadMessageAsync());
            _ = Task.Run(() => SendMessageAsync());

            await Task.Delay(-1);
        }        

        private async Task SendConnectAsync()
        {
            var memoryStream = new MemoryStream();
            using var writer = new BinaryWriter(memoryStream);

            writer.Write((byte)Opcode.HandleConnect);
            writer.Write(Encoding.ASCII.GetBytes(_username));

            await _stream.WriteAsync(memoryStream.ToArray());
        }

        private async Task ReadMessageAsync()
        {
            while (true)
            {
                var buffer = new byte[_client.ReceiveBufferSize];
                await _stream.ReadAsync(buffer);

                var packet = PacketManager.GetPacket(buffer);

                if (packet is MessagePacket messagePacket)
                {
                    if (messagePacket?.Message.Length > 0)
                    {
                        var previousColor = Console.ForegroundColor;
                        Console.ForegroundColor = (ConsoleColor)messagePacket.Color;
                        Console.WriteLine($"{messagePacket.Username}: {messagePacket.Message}");
                        Console.ForegroundColor = previousColor;
                    }
                }
                else if (packet is ServerResponsePacket responsePacket)
                {
                    if (responsePacket.Success)
                        Console.WriteLine($"\nSuccessfully connected to {_ip}:{_port}. Welcome to {responsePacket.ServerName}!\nThere {(responsePacket.OnlineCount == 1 ? "is 1 other user" : $"are {responsePacket.OnlineCount} other users")} online.\n");
                    else
                    {
                        Console.WriteLine($"\nFailed to connect. The server returned an error: {responsePacket.Message}");
                        Environment.Exit(0);
                    }
                }
                else if (packet is BroadcastPacket broadcastPacket)
                {
                    var previousColor = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.DarkMagenta;
                    Console.WriteLine(broadcastPacket.Message);
                    Console.ForegroundColor = previousColor;
                }
            }
        }

        private async Task SendMessageAsync()
        {
            while (true)
            {
                string msg = Console.ReadLine();

                if (!string.IsNullOrEmpty(msg))
                {
                    var memoryStream = new MemoryStream();
                    using var writer = new BinaryWriter(memoryStream);

                    // Opcode
                    writer.Write((byte)Opcode.HandleMessage);

                    // Message length
                    writer.Write((ushort)msg.Length);

                    // Random color
                    writer.Write(_color);

                    var buffer = new byte[16];
                    byte[] usernameBytes = Encoding.ASCII.GetBytes(_username);

                    for (int i = 0; i < usernameBytes.Length; i++)
                        buffer[i] = usernameBytes[i];

                    // Username
                    writer.Write(buffer);

                    // Message
                    writer.Write(Encoding.ASCII.GetBytes(msg));

                    await _stream.WriteAsync(memoryStream.ToArray());
                }
            }
        }

        private void AskIpAdressAndPort()
        {
            while (true)
            {
                Console.Write("Enter an IP and port to connect (e.g. 127.0.0.1:1337) or leave empty for localhost. -> ");

                string input = Console.ReadLine();

                if (!string.IsNullOrEmpty(input))
                {
                    try
                    {
                        string[] split = input.Split(':');
                        _ip = split[0];
                        _port = int.Parse(split[1]);
                        break;
                    }
                    catch
                    {
                        Console.Write("Incorrect format. ");
                    }
                }
                else
                {
                    _ip = "127.0.0.1";
                    _port = 1337;
                    break;
                }
            }
        }

        private void AskUsername()
        {
            do
            {
                Console.Write("What's your username? -> ");
                _username = Console.ReadLine();
            }
            while (string.IsNullOrEmpty(_username));
        }
    }
}
