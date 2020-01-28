using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using 

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
            sender.Connect(localEndPoint);
            Console.WriteLine("Socket connected to -> {0} ", localEndPoint);
        }

        public void Disconnect() {
            sender.Shutdown(SocketShutdown.Both);
            sender.Close();
        }

        //public void SendMessage(string message) => SendMessage(Encoding.ASCII.GetBytes(message), MessageType.String);

        public void SendMessage(byte[] message, MessageType messageType) {
            /*NetworkStream networkStream = new NetworkStream(sender);

            if (networkStream.CanWrite) {
                networkStream.Write( BitConverter.GetBytes((ushort)(message.Length + 1)));
                networkStream.WriteByte((byte)messageType);
                networkStream.Write(message, 0, message.Length);
            }

            networkStream.Dispose();*/

            byte[] messageSent = new byte[message.Length + 1];
            Buffer.BlockCopy(new[] { (byte)messageType }, 0, messageSent, 0, 1);
            Buffer.BlockCopy(message, 0, messageSent, 1, message.Length);

            try {
                sender.Send(messageSent);
            } catch (SocketException e) {

            }
        }
    }
}