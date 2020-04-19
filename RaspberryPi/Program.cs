using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SharedStuff;

namespace RaspberryPi
{
    class Program
    {
        const string ServerIPAddress = "127.0.0.1";

        static SocketClient socketClient;

        static void Main(string[] args)
        {
            socketClient = new SocketClient(ServerIPAddress);
            Console.WriteLine("Hello");
            socketClient.Connect();
            Console.WriteLine("Yes");

            SensorList.Initialize();
            try { Sak.InitalizeSaker(); } catch (Exception e) { Console.WriteLine(e); }

            Thread photoThread = new Thread(PhotoLoop);
            photoThread.Start();

            Dictionary<byte, Dictionary<byte, List<Sensor>>> sensorsByBusByAddr = new Dictionary<byte, Dictionary<byte, List<Sensor>>>();
            foreach (var sensor in SensorList.Sensors) {
                if (sensor is I2cSensor i2cSensor) {
                    if (i2cSensor.Device == null) continue;
                    sensorsByBusByAddr.GetValueCreateNew((byte) i2cSensor.Device.ConnectionSettings.BusId)
                        .GetValueCreateNew((byte) i2cSensor.Device.ConnectionSettings.DeviceAddress).Add(i2cSensor);
                } else {
                    sensorsByBusByAddr.GetValueCreateNew((byte)1).GetValueCreateNew((byte)1).Add(sensor);
                }
            }

            while (true) {
                DateTime now = DateTime.Now;

                Dictionary<byte, byte> errorCounts = new Dictionary<byte, byte>();

                List<Task> tasks = new List<Task>();
                foreach (var sensorsByAddr in sensorsByBusByAddr.Values) {
                    tasks.Add(new Task(() => { 
                        try {
                            foreach (var sensors in sensorsByAddr.Values) {
                                bool skipRest = false;
                                foreach (var sensor in sensors) {
                                    sensor.UpdateValues();
                                    if (skipRest) {
                                        foreach (SensorValue sensorValue in sensor.GetSensorValues()) { sensorValue.online = false; }
                                    } else if (!sensor.GetSensorValues()[0].online) {
                                        if (errorCounts.ChangeValueCreateDefault(sensor.SensorId, c => ++c) == 5) {
                                            Console.WriteLine("Startar om usb-hubben...");
                                            Bash("sudo uhubctl -l 1-1 -p 2 -a 0");
                                            Thread.Sleep(100);
                                            Bash("sudo uhubctl -l 1-1 -p 2 -a 1");
                                        }
                                        skipRest = true;
                                        continue;
                                    }
                                    errorCounts[sensor.SensorId] = 0;
                                }
                            }
                        } catch (Exception e) { Console.WriteLine(e); }
                    }));
                }
                tasks.ForEach(t => t.Start());
                tasks.ForEach(t => t.Wait());

                if (socketClient.sender.Connected) {
                    SendSensorData(SensorList.Sensors, socketClient, now);
                    Console.WriteLine("Connected");
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
                Bash("raspistill -o cam.jpg -q 40 -w 1640 -h 1232 -n");
                socketClient.SendMessage(File.ReadAllBytes("cam.jpg"), MessageType.Image1);
                File.Delete("cam.jpg");
            } catch (Exception e) {
                Console.WriteLine($"Kunde inte ta stillbild 1.\n{e}");
            }

            try {
                Bash("ssh pi@192.168.195.137 raspistill -o cam2.jpg -q 40 -w 1640 -h 1232 -n && scp pi@192.168.195.137:~/cam2.jpg .");
                socketClient.SendMessage(File.ReadAllBytes("cam2.jpg"), MessageType.Image2);
                File.Delete("cam2.jpg");
            } catch (Exception e) {
                Console.WriteLine($"Kunde inte ta stillbild 2.\n{e}");
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
