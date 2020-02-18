using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using SharedStuff;

namespace MainServer {
    class SocketServer {

        private const int packetSize = 65535;

        public static void RunServer() {

            IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddr = ipHost.AddressList[0];
            foreach (IPAddress ip in ipHost.AddressList) {
                if (ip.AddressFamily == AddressFamily.InterNetwork) {
                    ipAddr = ip;
                }
            }

            IPEndPoint localEndPoint = new IPEndPoint(ipAddr, 11111);
            Console.WriteLine($"Listening on {localEndPoint}");

            Socket listener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try {
                listener.Bind(localEndPoint);
                
                listener.Listen(1);
    
                Console.WriteLine("Waiting for connection ... ");
                Socket clientSocket = listener.Accept();

                int currentMessagePackageCount = 0, currentMessagePackagesSent = 0, currentMessageLength = 0;
                byte[] message = new byte[packetSize];
                while (true) {
                    byte[] packet = new byte[packetSize];

                    int length = clientSocket.Receive(packet);
                    MessageType messageType = (MessageType)packet[0];

                    if(currentMessagePackagesSent >= currentMessagePackageCount) {
                        currentMessagePackageCount = packet[1];
                        currentMessagePackagesSent = 0;
                        currentMessageLength = 0;

                        message = new byte[currentMessagePackageCount * (packetSize - 2)];
                    }

                    Console.WriteLine("length: " + length + " type: " + (int)messageType + " || " + currentMessagePackageCount + " : " + currentMessagePackagesSent + " : " + currentMessageLength);

                    Buffer.BlockCopy(packet, 2, message, currentMessageLength, length - 2);

                    Console.WriteLine("Message recieved epicly");

                    currentMessagePackagesSent++;
                    currentMessageLength += length - 2;

                    if (currentMessagePackagesSent >= currentMessagePackageCount) {
                        Span<byte> messageSpan = message.AsSpan<byte>(0, currentMessageLength);
                        HandleMessage(messageType, messageSpan.ToArray());
                    }
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
