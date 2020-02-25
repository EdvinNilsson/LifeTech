using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using SharedStuff;
using Iot.Device.CharacterLcd;

namespace RaspberryPi
{
    class Program
    {
        static SocketClient socketClient;
        static void Main(string[] args)
        {
            socketClient = new SocketClient("127.0.0.1");
            Console.WriteLine("Hello");
            socketClient.Connect();
            Console.WriteLine("Yes");

            SensorList.Initialize();

            Thread photoThread = new Thread(PhotoLoop);
            photoThread.Start();

            //using (var lcd = new Lcd1602(18, 5, new[] {6, 16, 20, 21}))
            //{
            //    lcd.Write("Hello World!");
            //}

            while (true) {
                DateTime now = DateTime.Now;
                foreach (var sensor in SensorList.sensors) {
                    sensor.UpdateValues();
                }

                if (socketClient.sender.Connected) {
                    SendSensorData(SensorList.sensors, socketClient, now);
                    Console.WriteLine("connected");
                } else {
                    socketClient.Connect();
                    Console.WriteLine("Not connected");
                }

                while (DateTime.Now.Second == now.Second) {
                    Thread.Sleep(1);
                }
            }
        }

        static void SendSensorData(Sensor[] sensors, SocketClient socketClient, DateTime now) {
            SensorData sensorData = new SensorData(now, sensors);
            foreach (var sensor in sensorData.Sensors) {
                Console.WriteLine($"{sensor.type} {sensor.value}");
            }
            socketClient.SendMessage(sensorData.Serialize(), MessageType.SensorData);
        }

        static void PhotoLoop() {
            var interval = TimeSpan.FromMinutes(15);
            while (true) {
                SendPhoto();
                var nextPhotoTime = MyMath.Round(DateTime.Now, interval);
                if (nextPhotoTime < DateTime.Now) nextPhotoTime += interval;
                Thread.Sleep(nextPhotoTime - DateTime.Now);
            }
        }

        static void SendPhoto() {
            try {
                Bash("raspistill -o cam.jpg");
                socketClient.SendMessage(File.ReadAllBytes("cam.jpg"), MessageType.Image); }
            catch (Exception e) {
                Console.WriteLine($"Kunde inte ta stillbild.\n{e}");
            }
        }

        static float NegativeFeedback(float expectedValue, float actualValue, float max = 10, float exponent = 2, float multiplier = 1)
        {
            return MathF.Min(MathF.Pow(expectedValue - actualValue, exponent) * multiplier, max);
        }

        public static string Bash(string cmd) {
            var escapedArgs = cmd.Replace("\"", "\\\"");

            var process = new Process() {
                StartInfo = new ProcessStartInfo {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{escapedArgs}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            process.Start();
            string result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return result;
        }
    }
}
