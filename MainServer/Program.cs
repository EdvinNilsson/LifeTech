using System;
using System.Threading;

namespace MainServer
{
    class Program
    {
        static void Main(string[] args)
        {
            DatabaseHandler.Initialize();
            Thread webServerThread = new Thread(WebServer.StartServer);
            Thread socketServerThread = new Thread(SocketServer.RunServer);

            webServerThread.Start();
            socketServerThread.Start();

            webServerThread.Join();
            socketServerThread.Join();
            Console.Read();
        }
    }
}