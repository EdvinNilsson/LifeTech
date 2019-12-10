using System;
using static MainServer.Webserver;

namespace MainServer {
    static class Routes {
        public static void Initialize() {
            Get("/", () => {
                return $"Hello there!\nThe tims is {DateTime.Now.ToLongTimeString()}.";
            });

            Get("/test", context => {
                SimpleTextResponse(context.Response, "Test");
            });

            Get("/test/*", id => {
                return "Id: " + id;
            });
        }
    }
}
