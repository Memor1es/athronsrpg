using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GTANetworkAPI;

namespace rpg
{
    public class http : Script
    {
        public static HttpListener listener;
        public static string url = "http://192.168.1.101:8080/";
        public static int pageViews = 0;
        public static int requestCount = 0;
        public static string pageData =
            "<!DOCTYPE>" +
            "<html>" +
            "  <head>" +
            "    <title>HttpListener Example</title>" +
            "  </head>" +
            "  <body>" +
            "    <p>Page Views: {0}</p>" +
            "    <form method=\"post\" action=\"shutdown\">" +
            "      <input type=\"submit\" value=\"Shutdown\" {1}>" +
            "    </form>" +
            "  </body>" +
            "</html>";


        public static async Task HandleIncomingConnections()
        {
            bool runServer = true;

            // While a user hasn't visited the `shutdown` url, keep on handling requests
            while (runServer)
            {
                // Will wait here until we hear from a connection
                HttpListenerContext ctx = await listener.GetContextAsync();

                // Peel out the requests and response objects
                HttpListenerRequest req = ctx.Request;
                HttpListenerResponse resp = ctx.Response;

                // Print out some info about the request
                Console.WriteLine("Request #: {0}", ++requestCount);
                Console.WriteLine(req.Url.ToString());
                Console.WriteLine(req.HttpMethod);
                Console.WriteLine(req.UserHostName);
                Console.WriteLine(req.UserAgent);
                Console.WriteLine();
                var pageData = "";
                // If `shutdown` url requested w/ POST, then shutdown the server after serving the page
                if ((req.HttpMethod == "GET"))
                {
                    switch (req.Url.AbsolutePath)
                    {
                        case "/kick":
                        {
                                if (req.QueryString.Get("player") != "")
                                {
                                    var player = playerManager.getPlayer(req.QueryString.Get("player"));
                                    if (player != null)
                                    {
                                        player.Kick("ai lua muie");
                                        pageData = $"Playerul {player.Name} a fost dat afara";
                                    }
                                    else
                                        pageData = $"Playerul nu este online.";

                                }
                             break;
                        }
                        case "/send":
                            {
                                if (req.QueryString.Get("msg") != "")
                                {
                                    NAPI.Chat.SendChatMessageToAll("SERVER: " + req.QueryString.Get("msg"));

                                }
                                break;
                            }
                        case "/playerList":
                        {
                                foreach (var item in NAPI.Pools.GetAllPlayers())
                                {
                                    pageData += $"Name: {item.Name} IP Address: {item.Address} Position {NAPI.Util.ToJson(item.Position)}";
                                }
                         }
                        break;

                    }
                   
                }

                // Make sure we don't increment the page views counter if `favicon.ico` is requested
                if (req.Url.AbsolutePath != "/favicon.ico")
                    pageViews += 1;

                // Write the response info
                string disableSubmit = !runServer ? "disabled" : "";
                byte[] data = Encoding.UTF8.GetBytes(pageData);
                resp.ContentType = "text/html";
                resp.ContentEncoding = Encoding.UTF8;
                resp.ContentLength64 = data.LongLength;

                // Write out to the response stream (asynchronously), then close it
                await resp.OutputStream.WriteAsync(data, 0, data.Length);
                resp.Close();
            }
        }

        [ServerEvent(Event.ResourceStart)]
        public void onHTTPStart()
        {

      
          /*  new Thread(() =>
            {

                Thread.CurrentThread.IsBackground = true;
                listener = new HttpListener();
                listener.Prefixes.Add(url);
                listener.Start();
                Console.WriteLine("Listening for connections on {0}", url);

                // Handle requests
                Task listenTask = HandleIncomingConnections();
                listenTask.GetAwaiter().GetResult();

                // Close the listener
                listener.Close();

            }).Start();*/
           
        }
    }
}
