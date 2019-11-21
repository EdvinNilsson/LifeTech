using System;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;

namespace RaspberryPi
{
    class Program
    {
        static void Main(string[] args)
        {
            var sensor = new MoistureSensor();

            SocketClient socketClient = new SocketClient("192.168.192.88");
            
            while (true)
            {
                while (!socketClient.sender.Connected)
                {
                    Console.WriteLine("Connecting to socket thing");
                    try
                    {
                        socketClient.Connect();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }

                    Thread.Sleep(1000);
                }

                socketClient.SendMessage("Hello there");

                sensor.UpdateValues();

                if (sensor.Online)
                    socketClient.SendMessage(sensor.Moisture.ToString());
                else
                    Console.WriteLine("Oh no, the flow sensor is not online");
                
                Thread.Sleep(1000);
            }

            socketClient.Disconnect();
        }

        static float NegativeFeedback(float expectedValue, float actualValue, float max = 10, float exponent = 2, float multiplier = 1)
        {
            return MathF.Min(MathF.Pow(expectedValue - actualValue, exponent) * multiplier, max);
        }
    }
}
