using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using System.Threading;

namespace MainServer {
    class Webserver {
        static HttpListener listener = new HttpListener();

        public static void StartServer() {
            listener.Prefixes.Add("http://localhost:8080/");
            listener.Start();
            listener.BeginGetContext(OnConnection, null);
        }

        static void OnConnection(IAsyncResult result) {
            listener.BeginGetContext(OnConnection, null);
            new Task(() => ProcessRequest(result)).Start();
        }

        static void ProcessRequest(IAsyncResult result) {
            HttpListenerContext context = listener.EndGetContext(result);
            Console.WriteLine(context.Request.Url.AbsolutePath);
            HttpListenerResponse response = context.Response;

            switch (context.Request.Url.AbsolutePath) {
                case "/":
                    IndexPage(response);
                    break;
                default:
                    response.StatusCode = 404;
                    Stream output = response.OutputStream;
                    output.Close();
                    break;
            }
        }

        static void IndexPage(HttpListenerResponse response) {
            string responseString = $"<HTML><BODY>Hello world! {DateTime.Now.ToLongTimeString()}</BODY></HTML>";
            SimpleTextResponse(response, responseString);
        }

        static void SimpleTextResponse(HttpListenerResponse response, string responseString) {
            byte[] buffer = Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Close();
        }
    }
}
