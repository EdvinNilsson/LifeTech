using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using SharedStuff;

namespace RaspberryPi {
    class SocketClient {

        public IPEndPoint localEndPoint;
        public Socket sender;

        private IPAddress ipAddress;

        public SocketClient(string ip) {
            ipAddress = IPAddress.Parse(ip);
            localEndPoint = new IPEndPoint(ipAddress, 11111);
            sender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Connect() {
            while (true) {
                try {
                    sender.Connect(localEndPoint);
                    Console.WriteLine("Socket connected to -> {0} ", localEndPoint);
                    return;
                }
                catch {
                    sender.Dispose();
                    sender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                    Console.WriteLine("Could not connect to socket server");
                }
            }
        }

        public void Disconnect() {
            sender.Shutdown(SocketShutdown.Both);
            sender.Close();
        }

        byte[] buffer = new byte[65535];
        public bool SendMessage(byte[] message, MessageType messageType)
        {
            buffer[0] = (byte) messageType;
            buffer[1] = (byte) ((message.Length - 1) / 65533 + 1);

            for (int i = 0, pos = 0; i < buffer[1]; ++i, pos += 65533)
            {
                int bytesLeft = message.Length - pos;
                Buffer.BlockCopy(message, pos, buffer, 2, bytesLeft <= 65533 ? bytesLeft : 65533);
                try
                {
                    if (i != buffer[1] - 1)
                        sender.Send(buffer);
                    else
                        sender.Send(buffer.AsSpan(0, bytesLeft));
                } catch (SocketException e) {
                    return false;
                }
            }

            return true;
        }
    }
}