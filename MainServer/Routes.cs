using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using static MainServer.WebServer;

namespace MainServer {
    static class Routes {
        public static void Initialize() {
            Get("/", context => {
                return "<p>Startsida</p>";
            });

            Get("/realtime", context => {
                return File.ReadAllText("Views/realtime.html", Encoding.UTF8);
            }, "Realtidsdata");

            Get("/about", context => {
                return File.ReadAllText("Views/about.html", Encoding.UTF8);
            }, "Om NTI Lifetech");

            Get("/get-sensordata", context => {
                byte[] sensorIds = Array.ConvertAll(context.Request.QueryString["sensors"].Split(','), c => byte.Parse(c));
                byte[] buffer = DatabaseHandler.GetSensorData(sensorIds);
                context.Response.ContentLength64 = buffer.Length;
                context.Response.ContentType = "text/plain";
                Stream output = context.Response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                output.Close();
            });

            Get("/get-latest-sensor-value", context => {
                byte[] sensorIds = Array.ConvertAll(context.Request.QueryString["sensors"].Split(','), c => byte.Parse(c));
                bool done = false;
                DatabaseHandler.OnReceivedSensorData += sensorData => {
                    try {
                        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                        byte[] buffer = Encoding.UTF8.GetBytes(string.Join(' ',
                            sensorData.Sensors.Where(c => sensorIds.Contains(c.id))
                                .Select(c => $"{c.id},{(byte)c.type},{c.value}")));
                        context.Response.ContentLength64 = buffer.Length;
                        context.Response.ContentType = "text/plain";
                        Stream output = context.Response.OutputStream;
                        output.Write(buffer, 0, buffer.Length);
                        output.Close();
                        done = true;
                    } catch { }
                };
                Thread.Sleep(1500);
                if (done) return;
                Stream output = context.Response.OutputStream;
                context.Response.StatusCode = 408;
                output.Close();
            });

            Get("/*", (context, groups) => {
                context.Response.StatusCode = 404;
                return "<h2>Sidan hittades inte.</h2>";
            }, "Sidan hittades inte");
        }
    }
}
