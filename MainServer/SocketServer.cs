using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using SharedStuff;

namespace MainServer {
    class SocketServer {

        public static void RunServer() {

            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse("0.0.0.0"), 11111);
            Console.WriteLine($"Listening on {localEndPoint}");

            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            while (true) {

                listener.Bind(localEndPoint);

                listener.Listen(1);

                Console.WriteLine("Waiting for connection ... ");
                Socket clientSocket = listener.Accept();

                byte[] message = new byte[0];

                MessageType messageType = 0;
                int packetLength = 0, bytesRead = 0;

                byte[] header = new byte[4];

                while (true) {
                    try {
                        byte[] packet = new byte[clientSocket.ReceiveBufferSize];

                        int length = clientSocket.Receive(packet);

                        if (length == 0) continue;

                        int offset = 0;
                        while (true) {
                            if (bytesRead == packetLength) {
                                if (length - offset < 4) {
                                    Buffer.BlockCopy(packet, offset, header, 0, length - offset);
                                    clientSocket.Receive(header, length - offset, 4 - (length - offset), SocketFlags.None);
                                } else {
                                    Buffer.BlockCopy(packet, offset, header, 0, 4);
                                }
                                packetLength = header[0] + (header[1] << 8) + (header[2] << 16);
                                bytesRead = 0;
                                messageType = (MessageType)header[3];
                                message = new byte[packetLength];
                                offset += 4;
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

                            if (length > read + offset) {
                                offset += read;
                            } else {
                                break;
                            }
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
            } catch (KeyNotFoundException e) {
                Console.WriteLine($"No handler for message type {(byte)messageType}. Resetting socket...");
                throw e;
            } catch (Exception e) {
                Console.WriteLine(e);
            }
        }

        public static void RegisterHandler(MessageType messageType, MessageDelegate messageDelegate) => messageHandlers[messageType] = messageDelegate;
    }
}
