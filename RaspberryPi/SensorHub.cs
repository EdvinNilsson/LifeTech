using System;
using System.Collections.Generic;
using System.Device.I2c;
using System.Text;

namespace RaspberryPi
{
    class SensorHub
    {
        public SensorHub(I2cConnectionSettings connectionSettings)
        {
            try
            {
                Device = I2cDevice.Create(connectionSettings);
            }
            catch (ArgumentException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public I2cDevice Device { get; }

        public Sensor[] Sensors { get; set; }

        public void WriteData(byte sensorIndex, byte[] bytes)
        {
            byte[] payload = new byte[bytes.Length + 1];
            payload[0] = sensorIndex;
            bytes.CopyTo(payload, 1);

            if (Device is { } device)
            {
                try
                {
                    device.Write(payload);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            else
                Console.WriteLine("Finns ingen enhet att skicka data till via i2c.");

        }

        void ReadData(byte[] bytes)
        {
            byte id = bytes[0];
            byte[] data = bytes[1..];

            if (Sensors[id] is { } sensor)
                sensor.OnReceiveSensorData(data);
            else
                Console.WriteLine("Ogiltig sensor");
        }
    }
}
