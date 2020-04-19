using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using SharedStuff;
using static MainServer.WebServer;

namespace MainServer {
    static class Routes {

        public static void Initialize() {
            Get("/", context => File.ReadAllText("Views/index.html", Encoding.UTF8));

            Get("/media", context => File.ReadAllText("Views/media.html", Encoding.UTF8), "Media");

            Get("/kamera", context => string.Format(File.ReadAllText("Views/camera.html", Encoding.UTF8),
                GetTimeString(DatabaseHandler.GetLatestImageInfo(1).CreationTime),
                GetTimeString(DatabaseHandler.GetLatestImageInfo(2).CreationTime)), "Kamera");

            Get("/kamera/get-latest-image", context => {
                byte imageId = byte.Parse(context.Request.QueryString["image"]);
                byte[] buffer = File.ReadAllBytes(DatabaseHandler.GetLatestImageInfo(imageId).FullName);
                context.Response.ContentLength64 = buffer.Length;
                context.Response.ContentType = "image/jpeg";
                Stream output = context.Response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                output.Close();
            });

            Get("/realtid", context => {
                string[] classes = new string[5];
                classes[(byte) DatabaseHandler.GetDataPeriod(context.Request.QueryString["senaste"])] = " active";
                return string.Format(File.ReadAllText("Views/realtime.html", Encoding.UTF8), classes);
            }, "Realtidsdata");

            Get("/realtid/get-sensordata", context => {
                byte[] sensorIds = Array.ConvertAll(context.Request.QueryString["sensors"].Split(','), byte.Parse);
                var dataPeriod = DatabaseHandler.GetDataPeriod(context.Request.QueryString["senaste"]);
                byte[] buffer = DatabaseHandler.GetSensorData(sensorIds, dataPeriod);
                context.Response.ContentLength64 = buffer.Length;
                context.Response.ContentType = "text/plain";
                Stream output = context.Response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                output.Close();
            });

            Get("/realtid/get-latest-sensor-value", context => {
                byte[] sensorIds = Array.ConvertAll(context.Request.QueryString["sensors"].Split(','), byte.Parse);
                bool done = false;
                context.Response.ContentType = "text/plain";
                void SendSensorData(SensorData sensorData) {
                    try {
                        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                        byte[] buffer = Encoding.UTF8.GetBytes(string.Join(' ', 
                            sensorData.Sensors.Where(c => sensorIds.Contains(c.id))
                            .Select(c => $"{c.id},{(byte) c.type},{c.value}")));
                        context.Response.ContentLength64 = buffer.Length;
                        Stream output = context.Response.OutputStream;
                        output.Write(buffer, 0, buffer.Length);
                        output.Close();
                        done = true;
                    } catch { }
                }
                DatabaseHandler.OnReceivedSensorData += SendSensorData;
                Thread.Sleep(1500);
                if (done) return;
                DatabaseHandler.OnReceivedSensorData -= SendSensorData;
                ServerError(context, 408);
            });

            Get("/realtid/get-sensor-names-units", context => {
                byte[] buffer = Encoding.UTF8.GetBytes(string.Join('|', 
                    SensorList.Sensors.Select(c => $"{c.SensorId},{c.SensorName}")) + '#' +
                    string.Join(',', SensorList.Units) + '#' + string.Join(',', SensorList.Readings));
                context.Response.ContentLength64 = buffer.Length;
                context.Response.ContentType = "text/plain";
                Stream output = context.Response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                output.Close();
            });

            Get("/*", (context, groups) => {
                context.Response.StatusCode = 404;
                return "<article><h2>Sidan hittades inte.</h2></article>";
            }, "Sidan hittades inte");
        }

        static string GetTimeString(DateTime dateTime) {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("sv-SE");
            return DateTime.Now - dateTime > TimeSpan.FromHours(1)
                ? $"{dateTime.ToLongDateString()} kl. {dateTime.ToShortTimeString()}"
                : $"kl. {dateTime.ToShortTimeString()}";
        }
    }
}
