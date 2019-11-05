using System;
using System.Threading;

namespace MainServer
{
    class Program
    {
        //Negative feedback loop

        static void Main(string[] args)
        {
            Thread webserverThread = new Thread(new ThreadStart(Webserver.StartServer));
            Thread socketServerThread = new Thread(new ThreadStart(SocketServer.RunServer));

            webserverThread.Start();
            socketServerThread.Start();

            webserverThread.Join();
            socketServerThread.Join();
            Console.Read();
        }
    }
}