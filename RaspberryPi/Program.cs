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
            /*
            I2cDevice device = I2cDevice.Create(new I2cConnectionSettings(1, 8));
            byte[] command = {15, 124};
            byte[] result = new byte[4];
            device.WriteRead(command, result);

            foreach (var b in result)
            {
                Console.WriteLine(b);
            }
            */

            var flowSensor = new FlowSensor(0x4D);
            flowSensor.UpdateValues();
            if (flowSensor.Online)
                Console.WriteLine(flowSensor.FlowRate);

            Console.Read();
        }
    }
}
