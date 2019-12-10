using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MainServer {
    class Webserver {
        static HttpListener listener = new HttpListener();

        public static void StartServer() {
            Routes.Initialize();
            listener.Prefixes.Add("http://localhost:8080/");
            //listener.Prefixes.Add("http://*:8080/");
            listener.Start();
            listener.BeginGetContext(OnConnection, null);
        }

        static void OnConnection(IAsyncResult result) {
            listener.BeginGetContext(OnConnection, null);
            new Task(() => ProcessRequest(result)).Start();
        }

        static void ProcessRequest(IAsyncResult result) {
            HttpListenerContext context = listener.EndGetContext(result);
            
            switch (context.Request.HttpMethod)
            {
                case "GET":
                    if (!InvokePath(gets, context))
                        goto default;
                    break;
                case "POST":
                    if (!InvokePath(posts, context))
                        goto default;
                    break;
                default:
                    context.Response.StatusCode = 404;
                    Stream output = context.Response.OutputStream;
                    output.Close();
                    break;
            }
        }

        static bool InvokePath(Dictionary<string, Action<HttpListenerContext>> dic, HttpListenerContext context) {
            foreach (var item in dic) {
                if (Regex.IsMatch(context.Request.Url.AbsolutePath, item.Key)) {
                    item.Value(context);
                    return true;
                }
            }
            return false;
        }

        public static void SimpleTextResponse(HttpListenerResponse response, string responseString) {
            byte[] buffer = Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Close();
        }

        static string ToRegex(string input) => $"^{input.Replace("/", @"\/").Replace("*", ".*")}$";

        static readonly Dictionary<string, Action<HttpListenerContext>> gets = new Dictionary<string, Action<HttpListenerContext>>();
        public static void Get(string path, Action<HttpListenerContext> action) => gets[ToRegex(path)] = action;
        public static void Get(string path, Func<string> func) {
            gets[ToRegex(path)] = delegate (HttpListenerContext context) { SimpleTextResponse(context.Response, func()); };
        }
        public static void Get(string path, Func<string, string> func) {
            gets[ToRegex(path)] = delegate (HttpListenerContext context) { SimpleTextResponse(context.Response, func(GetId(context))); };
        }

        static readonly Dictionary<string, Action<HttpListenerContext>> posts = new Dictionary<string, Action<HttpListenerContext>>();
        public static void Post(string path, Action<HttpListenerContext> action) => posts[ToRegex(path)] = action;

        public static string GetId(HttpListenerContext context) => context.Request.Url.AbsolutePath.Split('/').Last();
    }
}
