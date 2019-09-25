using System;
using System.Device.I2c;

namespace RaspberryPi
{
    class Program
    {
        static void Main(string[] args)
        {
            SensorHub sensorHub = new SensorHub(new I2cConnectionSettings(0, 0)) {Sensors = new Sensor[1]};
            sensorHub.Sensors[0] = new FlowSensor(sensorHub, 0);
            FlowSensor flowSensor = (FlowSensor)sensorHub.Sensors[0];
            flowSensor.SendFrequency = 60;


            Console.WriteLine("Hello World!");
        }
    }
}
