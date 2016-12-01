using System;
using System.Runtime.InteropServices;
using Flow.Handlers;
using Flow.Responders;

namespace Flow.FileServer
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var router = new Router(8080, (exception, request) =>
            {
                var dump = ((IDumpable<RequestInfo.RequestInfoDump>) request).Dump();
                if (dump.Response == null)
                    request.Respond(500);
                dump.Response.StreamText(exception.ToString());
                return false;
            });
            router
                .If(request =>
                {
                    Console.WriteLine(request.Client.Client.RemoteEndPoint);
                    return false;
                })
                .RespondWith(null)
                .AddRest<string>(Root)
                .If(request => request.Path.StartsWith("/test"))
                .RespondWith(request => request.Respond(200).StreamText("yay"))
                .If(request => true)
                .RespondWith(request =>
                    request
                        .Respond(404)
                        .StreamText("<html><head><title>404</title></head><body><h1>404 Not Found</h1></body></html>",
                            "text/html")
                );
            router.Start();
        }

        private static void Root(Request request, [DefaultParameterValue("root")] string root)
        {
            request
                .Respond(200)
                .StreamText("root, you reached it!");
        }
    }
}