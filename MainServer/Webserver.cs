using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using MimeTypes;

namespace MainServer {
    class WebServer {
        static HttpListener listener = new HttpListener();
        static string publicPath;

        public static void StartServer() {
            Routes.Initialize();
            publicPath = Path.GetFullPath("Public");
            listener.Prefixes.Add(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "http://localhost:8080/" : "http://*:5678/");
            listener.Start();
            listener.BeginGetContext(OnConnection, null);
        }

        static bool StaticFile(string path, HttpListenerContext context) {
            string absolutePath = Path.GetFullPath(publicPath + path);
            if (!absolutePath.StartsWith(publicPath) || !File.Exists(absolutePath)) return false;
            byte[] buffer = File.ReadAllBytes(absolutePath);
            context.Response.ContentLength64 = buffer.Length;
            context.Response.ContentType = MimeTypeMap.GetMimeType(Path.GetExtension(absolutePath));
            Stream output = context.Response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Close();
            return true;
        }

        static void OnConnection(IAsyncResult result) {
            listener.BeginGetContext(OnConnection, null);
            new Task(() => ProcessRequest(result)).Start();
        }

        static void ProcessRequest(IAsyncResult result) {
            HttpListenerContext context = listener.EndGetContext(result);
            Console.WriteLine($"{DateTime.Now.ToLongTimeString()} {context.Request.Headers["CF-Connecting-IP"]} {context.Request.Url.AbsolutePath}");
            try {
                context.Response.Headers.Set(HttpResponseHeader.Server, string.Empty);
                switch (context.Request.HttpMethod) {
                    case "GET":
                        if (gets.TryGetValue(context.Request.Url.AbsolutePath, out var value))
                            value.Invoke(context);
                        else if (StaticFile(context.Request.Url.AbsolutePath, context)) { }
                        else if (!InvokeDynamicPath(dynamicGets, context))
                            goto default;
                        break;
                    case "POST":
                        if (!InvokeDynamicPath(posts, context))
                            goto default;
                        break;
                    default:
                        ServerError(context, 404);
                        break;
                }
            }
            catch (HttpListenerException) { }
            catch (Exception e) {
                Console.WriteLine(e);
                ServerError(context, 500);
            }
        }

        public static void ServerError(HttpListenerContext context, int errorCode) {
            context.Response.StatusCode = errorCode;
            Stream output = context.Response.OutputStream;
            output.Close();
        }

        static bool InvokeDynamicPath(Dictionary<string, Action<HttpListenerContext, string[]>> dic, HttpListenerContext context) {
            foreach (var item in dic) {
                var match = Regex.Match(context.Request.Url.AbsolutePath, item.Key);
                if (match.Success) {
                    string[] groups = new string[match.Groups.Count - 1];
                    for (int i = 0; i < groups.Length; ++i) {
                        groups[i] = match.Groups[i + 1].Value;
                    }
                    item.Value(context, groups);
                    return true;
                }
            }
            return false;
        }

        public static void SimpleTextResponse(HttpListenerResponse response, string responseString) {
            byte[] buffer = Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            response.ContentType = "text/html";
            Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Close();
        }

        public static NameValueCollection GetParameters(HttpListenerContext context) {
            switch (context.Request.HttpMethod) {
                case "GET": return context.Request.QueryString;
                case "POST":
                    Stream input = context.Request.InputStream;
                    byte[] buffer = new byte[context.Request.ContentLength64];
                    input.Read(buffer, 0, buffer.Length);
                    input.Close();
                    return HttpUtility.ParseQueryString(Encoding.UTF8.GetString(buffer));
            }
            return null;
        }

        public static void Redirect(HttpListenerContext context, string url) {
            Stream output = context.Response.OutputStream;
            context.Response.Redirect(url);
            output.Close();
        }

        static string ToRegex(string input) => $"^{input.Replace("/", @"\/").Replace("*", "(.*)")}$";

        static Dictionary<string, Action<HttpListenerContext, string[]>> dynamicGets = new Dictionary<string, Action<HttpListenerContext, string[]>>();
        static Dictionary<string, Action<HttpListenerContext>> gets = new Dictionary<string, Action<HttpListenerContext>>();

        public static void Get(string path, Action<HttpListenerContext> action) => gets[path] = action;
        public static void Get(string path, Action<HttpListenerContext, string[]> action) => dynamicGets[ToRegex(path)] = action;
        public static void Get(string path, Func<HttpListenerContext, string> func, string title = null) =>
            gets[path] = delegate (HttpListenerContext context) { SimpleTextResponse(context.Response, GenerateHTML(func(context), title)); };
        
        public static void Get(string path, Func<HttpListenerContext, string[], string> func, string title = null) =>
            dynamicGets[ToRegex(path)] = delegate (HttpListenerContext context, string[] groups)
                { SimpleTextResponse(context.Response, GenerateHTML(func(context, groups), title)); };

        static Dictionary<string, Action<HttpListenerContext, string[]>> posts = new Dictionary<string, Action<HttpListenerContext, string[]>>();
        public static void Post(string path, Action<HttpListenerContext, string[]> action) => posts[ToRegex(path)] = action;

        public static string GenerateHTML(string content, string title = null) {
            StringBuilder sb = new StringBuilder();
            return sb.AppendFormat(File.ReadAllText("Views/layout.html"), title == null ? "NTI Life Tech" : $"{title} - NTI Life Tech", content)
                .Replace("\n", string.Empty).Replace("\t", string.Empty).ToString();
        }
    }
}
