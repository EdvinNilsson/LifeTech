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

            while (true)
            {
                sensor.UpdateValues();
                if (sensor.Online)
                    Console.WriteLine(sensor.Moisture);

                Thread.Sleep(1000);
            }

            Console.Read();
        }

        static float NegativeFeedback(float expectedValue, float actualValue, float max = 10, float exponent = 2, float multiplier = 1)
        {
            return MathF.Min(MathF.Pow(expectedValue - actualValue, exponent) * multiplier, max);
        }
    }
}
