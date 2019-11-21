using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace RaspberryPi
{
    class SocketClient
    {

        public IPEndPoint localEndPoint;
        public Socket sender;

        private IPAddress ipAddress;

        public SocketClient(string ip)
        {
            ipAddress = IPAddress.Parse(ip);
            localEndPoint = new IPEndPoint(ipAddress, 11111);
            sender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Connect()
        {
            sender.Connect(localEndPoint);
            Console.WriteLine("Socket connected to -> {0} ", localEndPoint);
        }

        public void Disconnect()
        {
            sender.Shutdown(SocketShutdown.Both);
            sender.Close();
        }

        public string SendMessage(string message)
        {
            byte[] messageSent = Encoding.ASCII.GetBytes(message + "<EOF>");
            int byteSent = sender.Send(messageSent);

            byte[] messageReceived = new byte[1024];

            int byteRecv = sender.Receive(messageReceived);
            Console.WriteLine("Message from Server -> {0}", Encoding.ASCII.GetString(messageReceived, 0, byteRecv));

            return messageReceived.ToString();
        }

    }
}