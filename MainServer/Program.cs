using System;
using System.Threading;

namespace MainServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread webServerThread = new Thread(WebServer.StartServer);
            Thread socketServerThread = new Thread(SocketServer.RunServer);

            webServerThread.Start();
            DatabaseHandler.Initialize();
            socketServerThread.Start();
            DatabaseHandler.FillAllCacheArrays();


            webServerThread.Join();
            socketServerThread.Join();
            Console.Read();
        }
    }
}