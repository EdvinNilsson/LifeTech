using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;

namespace MainServer {
    class SocketServer {

        public static void RunServer() {
            IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress test in ipHost.AddressList) {
                Console.WriteLine(test.ToString());
            }

            IPAddress ipAddr = ipHost.AddressList[7];
            Console.WriteLine(ipAddr.ToString());
            IPEndPoint localEndPoint = new IPEndPoint(ipAddr, 11111);

            Console.WriteLine(ipAddr.AddressFamily);

            Socket listener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try {
                listener.Bind(localEndPoint);
                
                listener.Listen(1);
    
                Console.WriteLine("Waiting for connection ... ");
                Socket clientSocket = listener.Accept();

                while (true) {
                    byte[] bytes = new byte[65535];

                    int length = clientSocket.Receive(bytes);
                    byte messageType = bytes[0];

                    Console.WriteLine("length: " + length + " type: " + (int)messageType);

                    Span<byte> bytesSpan = bytes.AsSpan<byte>(1, length - 1);

                    Console.WriteLine("Message recieved epicly");

                    //Console.WriteLine("Text received -> {0} ", data);
                    //byte[] message = Encoding.ASCII.GetBytes("Test Server");

                    //clientSocket.Send(message);

                    //clientSocket.Shutdown(SocketShutdown.Both);
                    //clientSocket.Close();
                }
            }

            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
