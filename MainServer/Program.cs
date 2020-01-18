using System;
using System.Threading;

namespace MainServer
{
    class Program
    {
        //Negative feedback loop

        static void Main(string[] args)
        {
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