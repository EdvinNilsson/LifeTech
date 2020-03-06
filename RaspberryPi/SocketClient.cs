using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
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

        bool isSending = false;
        byte[] buffer = new byte[4];
        public bool SendMessage(byte[] message, MessageType messageType) {
            uint length = (uint) message.Length;
            buffer[0] = (byte) length;
            buffer[1] = (byte) (length >> 8);
            buffer[2] = (byte) (length >> 16);
            buffer[3] = (byte) messageType;
            while (isSending) {
                Thread.Sleep(1);
            }
            try {
                isSending = true;
                sender.Send(buffer);
                sender.Send(message);
                isSending = false;
            }
            catch {
                isSending = false;
                return false;
            }
            return true;
        }
    }
}