using System;
using System.IO;
using System.Text;
using static MainServer.WebServer;

namespace MainServer {
    static class Routes {
        public static void Initialize() {
            Get("/", context => {
                return "<p>Startsida</p>";
            });

            Get("/realtime", context => {
                return "<p>Under konstruktion</p>";
            }, "Realtidsdata");

            Get("/about", context => {
                return File.ReadAllText("Views/about.html", Encoding.UTF8);
            }, "Om NTI Lifetech");

            Get("/*", (context, groups) => {
                context.Response.StatusCode = 404;
                return "<h2>Sidan hittades inte.</h2>";
            }, "Sidan hittades inte");
        }
    }
}
