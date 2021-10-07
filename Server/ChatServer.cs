using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Packets;

namespace ChatProgram
{
    public class ChatServer
    {
        private readonly TcpListener _listener;

        private readonly Config _config;
        private readonly int _port;
        private readonly List<Tuple<TcpClient, string>> _clients = new();

        public ChatServer()
        {
            _config = JsonSerializer.Deserialize<Config>(File.ReadAllText("data/config.json"));
            _port = _config.Port;
            _listener = new TcpListener(IPAddress.Any, _port);
        }

        public async Task StartAsync()
        {
            _listener.Start();

            Logger.Log($"Started server on port {_port}");

            while (true)
            {
                var client = await _listener.AcceptTcpClientAsync();
                _ = Task.Run(() => HandleClientAsync(client));                                
            }
        }

        private async Task BroadcastAsync(string message, string[] usernamesToSkip = null)
        {
            var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);

            writer.Write((byte)Opcode.HandleBroadcast);
            writer.Write((ushort)message.Length);
            writer.Write(Encoding.ASCII.GetBytes(message));

            usernamesToSkip ??= new string[0];

            foreach (TcpClient cl in _clients.Where(x => !usernamesToSkip.Contains(x.Item2)).Select(x => x.Item1))
                await cl.GetStream().WriteAsync(stream.ToArray());
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            NetworkStream stream = client.GetStream();

            while (true)
            {
                try
                {
                    var buffer = new byte[client.ReceiveBufferSize];

                    try
                    {
                        await stream.ReadAsync(buffer.AsMemory(0, client.ReceiveBufferSize));
                    }
                    catch
                    {
                        string username = GetUsername(client);
                        await BroadcastAsync(_config.GoodbyeMessage.Replace("[USERNAME]", username), new[] { username });
                        RemoveClient(client);
                        return;
                    }

                    var packet = PacketManager.GetPacket(buffer);

                    if (packet is MessagePacket msgPacket)
                    {
                        foreach (var cl in _clients.Where(x => x.Item2 != msgPacket.Username))
                            if (string.IsNullOrEmpty(msgPacket.Recipient) || cl.Item2 == msgPacket.Recipient)
                                await cl.Item1.GetStream().WriteAsync(buffer);
                    }
                    else if (packet is ConnectPacket connPacket)
                    {
                        bool success;
                        string message;

                        if (!_clients.Any(x => x.Item2 == connPacket.Username))
                        {                           
                            success = true;
                            message = "Successfully connected.";
                        }
                        else
                        {
                            success = false;
                            message = "Username already exists.";
                        }

                        var memoryStream = new MemoryStream();
                        using var writer = new BinaryWriter(memoryStream);

                        writer.Write((byte)Opcode.HandleServerResponse);
                        writer.Write((ushort)message.Length);
                        writer.Write(success);
                        writer.Write((ushort)_clients.Count);

                        var serverNameBuffer = new byte[32];
                        byte[] serverNameBytes = Encoding.ASCII.GetBytes(_config.ServerName);

                        for (int i = 0; i < serverNameBytes.Length; i++)
                            serverNameBuffer[i] = serverNameBytes[i];

                        writer.Write(serverNameBuffer);

                        writer.Write(Encoding.ASCII.GetBytes(message));

                        await stream.WriteAsync(memoryStream.ToArray());

                        if (success)
                        {
                            _clients.Add(Tuple.Create(client, connPacket.Username));
                            Logger.Log($"{client.Client.RemoteEndPoint as IPEndPoint} connected as {connPacket.Username}");
                            await BroadcastAsync(_config.WelcomeMessage.Replace("[USERNAME]", connPacket.Username), new[] { connPacket.Username });
                        }
                        else
                            Logger.Log($"{client.Client.RemoteEndPoint as IPEndPoint} tried to connect but received an error: {message}");
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        private string GetUsername(TcpClient client)
            => _clients.FirstOrDefault(x => (x.Item1.Client.RemoteEndPoint as IPEndPoint).ToString() == (client.Client.RemoteEndPoint as IPEndPoint).ToString()).Item2;

        private void RemoveClient(TcpClient client)
        {
            if (_clients.FirstOrDefault(x => x.Item1 == client) is Tuple<TcpClient, string> clientTuple)
            {
                _clients.Remove(clientTuple);
                Logger.Log($"{client.Client.RemoteEndPoint as IPEndPoint} disconnected ({clientTuple.Item2})");
            }
        }
    }
}
