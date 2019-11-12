using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace RaspberryPi {
    class SocketClient {

        public IPEndPoint localEndPoint;
        public Socket sender;

        public SocketClient(string ip) {
            IPHostEntry ipHost = Dns.GetHostEntry(ip);
            IPAddress ipAddr = ipHost.AddressList[0];
            localEndPoint = new IPEndPoint(ipAddr, 11111);

            sender = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        }

        public void connect() {
            try {
                sender.Connect(localEndPoint);
                Console.WriteLine("Socket connected to -> {0} ", sender.RemoteEndPoint.ToString());
            }
            catch {
                return;
            }
        }

        public void dissconnect() {
            sender.Shutdown(SocketShutdown.Both);
            sender.Close();
        }

        public string sendMessage(string message) {
            byte[] messageSent = Encoding.ASCII.GetBytes(message + "<EOF>");
            int byteSent = sender.Send(messageSent);

            byte[] messageReceived = new byte[1024];

            int byteRecv = sender.Receive(messageReceived);
            Console.WriteLine("Message from Server -> {0}", Encoding.ASCII.GetString(messageReceived, 0, byteRecv));

            return messageReceived.ToString();
        }

    }
}
