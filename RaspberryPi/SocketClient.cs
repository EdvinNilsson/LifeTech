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

        //public void SendMessage(string message) => SendMessage(Encoding.ASCII.GetBytes(message), MessageType.String);

        public bool SendMessage(byte[] message, MessageType messageType) {

            byte[] messageSent = new byte[message.Length + 1];
            Buffer.BlockCopy(new[] { (byte)messageType }, 0, messageSent, 0, 1);
            Buffer.BlockCopy(message, 0, messageSent, 1, message.Length);

            try {
                sender.Send(messageSent);
            } catch (SocketException e) {
                return false;
            }

            return true;
        }
    }
}