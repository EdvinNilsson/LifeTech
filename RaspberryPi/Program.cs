using System;
using System.Collections.Generic;
using System.Device.I2c;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SharedStuff;
using Iot.Device.CharacterLcd;

namespace RaspberryPi
{
    class Program
    {
        static void SendSensorData(Sensor[] sensors, SocketClient socketClient)
        {
            SensorData sensorData = new SensorData(DateTime.Now, sensors);
            foreach (var sensor in sensorData.Sensors)
            {
                Console.WriteLine( $"{sensor.type} {sensor.value}");
            }
            socketClient.SendMessage(sensorData.Serialize(), MessageType.SensorData);
        }

        static void Main(string[] args)
        {
            SocketClient socketClient = new SocketClient("192.168.192.160");
            socketClient.Connect();

            SensorList.Initialize();

            using (var lcd = new Lcd1602(18, 5, new[] {6, 16, 20, 21}))
            {
                lcd.Write("Hello World!");
            }

            while (true)
            {
                foreach (var sensor in SensorList.sensors)
                {
                    sensor.UpdateValues();
                }

                SendSensorData(SensorList.sensors, socketClient);

                int sec = DateTime.Now.Second;
                while (DateTime.Now.Second == sec)
                {
                    Thread.Sleep(1);
                }
            }

            socketClient.Disconnect();
        }

        static float NegativeFeedback(float expectedValue, float actualValue, float max = 10, float exponent = 2, float multiplier = 1)
        {
            return MathF.Min(MathF.Pow(expectedValue - actualValue, exponent) * multiplier, max);
        }
    }
}
