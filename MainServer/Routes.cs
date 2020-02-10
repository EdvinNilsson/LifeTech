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
            Get("/", context => {
                return "<article><p>Startsida</p></article>";
            });

            Get("/realtid", context => {
                string[] classes = new string[5];
                classes[(byte)DatabaseHandler.GetDataPeriod(context.Request.QueryString["senaste"])] = " active";
                return string.Format(File.ReadAllText("Views/realtime.html", Encoding.UTF8), classes);
            }, "Realtidsdata");

            Get("/om", context => {
                return File.ReadAllText("Views/about.html", Encoding.UTF8);
            }, "Om NTI Life Tech");

            Get("/realtid/get-sensordata", context => {
                byte[] sensorIds = Array.ConvertAll(context.Request.QueryString["sensors"].Split(','), c => byte.Parse(c));
                var dataMode = DatabaseHandler.GetDataPeriod(context.Request.QueryString["senaste"]);
                byte[] buffer = DatabaseHandler.GetSensorData(sensorIds, dataMode);
                context.Response.ContentLength64 = buffer.Length;
                context.Response.ContentType = "text/plain";
                Stream output = context.Response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                output.Close();
            });

            Get("/realtid/get-latest-sensor-value", context => {
                byte[] sensorIds = Array.ConvertAll(context.Request.QueryString["sensors"].Split(','), c => byte.Parse(c));
                bool done = false;
                context.Response.ContentType = "text/plain";
                void SendSensorData(SensorData sensorData) {
                    try {
                        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                        byte[] buffer = Encoding.UTF8.GetBytes(string.Join(' ', sensorData.Sensors.Where(c => sensorIds.Contains(c.id))
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
                    SensorList.sensors.Select(c => $"{c.SensorId},{c.SensorName}")) + '#' + string.Join(',', SensorList.Units));
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
    }
}
