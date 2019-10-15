using System;
using System.Threading;

namespace MainServer
{
    class Program
    {

        public static void test() {
            Console.WriteLine("hello");
        }
        //Negative feedback loop

        static void Main(string[] args)
        {
            Thread webserverThread = new Thread(new ThreadStart(Webserver.StartServer));
            Thread testThread = new Thread(new ThreadStart(test));

            webserverThread.Start();
            testThread.Start();

            webserverThread.Join();
            testThread.Join();
            Console.Read();
        }
    }
}