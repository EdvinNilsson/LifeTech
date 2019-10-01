using System;
using System.Device.I2c;

namespace RaspberryPi
{
    class Program
    {
        static void Main(string[] args)
        {
            /*
             * GND 6 => GND
             * SDA 3 => A4
             * SCL 5 => A5
             */
            I2cDevice device = I2cDevice.Create(new I2cConnectionSettings(1, 8));
            byte[] command = {15, 124};
            byte[] result = new byte[4];
            device.WriteRead(command, result);
            foreach (var b in result)
            {
                Console.WriteLine(b);
            }

            /*
            SensorHub sensorHub = new SensorHub(new I2cConnectionSettings(1, 0x00)) {Sensors = new Sensor[1]};
            sensorHub.Sensors[0] = new FlowSensor(sensorHub, 0);
            FlowSensor flowSensor = (FlowSensor)sensorHub.Sensors[0];
            flowSensor.SendFrequency = 60;
            */

            Console.WriteLine("Hello World!");
            Console.Read();
        }
    }
}
