using System;
using System.Net;
using System.Linq;
using Flow.Responders;
using System.Text;
using System.IO;
using System.Threading;
using Flow.Handlers;
using System.Collections.Generic;
using Flow.Fetchers;
using System.Runtime.InteropServices;

namespace Flow.FileServer
{
	static class Program
	{
		static void Main(string[] args)
		{
			var router = new Router(8080, (exception, request) => {
				var dump = ((IDumpable<RequestInfo.RequestInfoDump>)request).Dump();
				if(dump.Response == null)
					request.Respond(500);
				dump.Response.StreamText(exception.ToString());
				return false;
			});
			router
				.If(request => { Console.WriteLine(request.Client.Client.RemoteEndPoint); return false; })
				.RespondWith(null)
				.AddRest<string>(root)
				.If(request => true)
				.RespondWith(request =>
					request
						.Respond(404)
						.StreamText("<html><head><title>404</title></head><body><h1>404 Not Found</h1></body></html>", "text/html")
				);
			router.Start();
		}
		
		static void root(Request request, [DefaultParameterValueAttribute("root")]string root)
		{
			request
				.Respond(200)
				.StreamText("root, you reached it!");
		}
	}
}