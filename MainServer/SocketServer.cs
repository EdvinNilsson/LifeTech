using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using SharedStuff;

namespace MainServer {
    class SocketServer {

        public static void RunServer() {


            IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress test in ipHost.AddressList) {
                Console.WriteLine(test.ToString());
            }

            IPAddress ipAddr = ipHost.AddressList[3];
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
                    MessageType messageType = (MessageType)bytes[0];

                    Console.WriteLine("length: " + length + " type: " + (int)messageType);

                    Span<byte> bytesSpan = bytes.AsSpan<byte>(1, length - 1);

                    Console.WriteLine("Message recieved epicly");

                    //Console.WriteLine("Text received -> {0} ", data);
                    //byte[] message = Encoding.ASCII.GetBytes("Test Server");

                    //clientSocket.Send(message);

                    //clientSocket.Shutdown(SocketShutdown.Both);
                    //clientSocket.Close();

                    HandleMessage((MessageType)messageType, bytesSpan.ToArray());
                }
            }

            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }

        public delegate void MessageDelegate(byte[] message);
        static readonly Dictionary<MessageType, List<MessageDelegate>> MessageHandlers = new Dictionary<MessageType, List<MessageDelegate>>();

        static void HandleMessage(MessageType messageType, byte[] message) {
            try {
                foreach (var messageHandler in MessageHandlers[messageType]) {
                    messageHandler(message);
                }
            } catch (KeyNotFoundException) {
                Console.WriteLine($"No handler for message type {(byte)messageType}.");
            }
        }

        public static void RegisterHandler(MessageType messageType, MessageDelegate messageDelegate) {
            try {
                MessageHandlers[messageType].Add(messageDelegate);
            } catch (KeyNotFoundException) {
                MessageHandlers.Add(messageType, new List<MessageDelegate>());
                MessageHandlers[messageType].Add(messageDelegate);
            }
        }
    }
}
