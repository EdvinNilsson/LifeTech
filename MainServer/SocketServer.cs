using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using SharedStuff;

namespace MainServer {
    class SocketServer {

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

            while (true) {

                listener.Bind(localEndPoint);
            
                listener.Listen(1);
    
                Console.WriteLine("Waiting for connection ... ");
                Socket clientSocket = listener.Accept();

                byte[] message = new byte[0];

                MessageType messageType = 0;
                int packetLength = 0, bytesRead = 0;

                while (true) {
                    try {
                        byte[] packet = new byte[clientSocket.ReceiveBufferSize];

                        int length = clientSocket.Receive(packet);

                        int offset = 0;
                        if (bytesRead == packetLength) {
                            packetLength = packet[0] + (packet[1] << 8) + (packet[2] << 16);
                            bytesRead = 0;
                            messageType = (MessageType)packet[3];
                            message = new byte[packetLength];
                            offset = 4;
                        }

                        int read = MyMath.Min(packetLength - bytesRead, length - offset);
                        if (read > 0) {
                            Buffer.BlockCopy(packet, offset, message, bytesRead, read);
                            bytesRead += read;
                        }

                        if (bytesRead == packetLength) {
                            Console.WriteLine($"Message received epicly, length: {packetLength} type: {messageType}");
                            HandleMessage(messageType, message);
                        }
                    } catch (Exception e) {
                        Console.WriteLine(e);
                        clientSocket.Dispose();
                        listener.Listen(1);
                        Console.WriteLine("Waiting for connection ... ");
                        clientSocket = listener.Accept();
                    }
                }
            }
        }

        public delegate void MessageDelegate(byte[] message);
        static Dictionary<MessageType, MessageDelegate> messageHandlers = new Dictionary<MessageType, MessageDelegate>();

        static void HandleMessage(MessageType messageType, byte[] message) {
            try {
                messageHandlers[messageType](message);
            } catch (KeyNotFoundException) {
                Console.WriteLine($"No handler for message type {(byte)messageType}.");
            } catch (Exception e) {
                Console.WriteLine(e);
            }
        }

        public static void RegisterHandler(MessageType messageType, MessageDelegate messageDelegate) => messageHandlers[messageType] = messageDelegate;
    }
}
