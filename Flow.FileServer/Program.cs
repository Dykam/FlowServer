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

namespace Flow.FileServer
{
	static class Program
	{
		static void Main(string[] args)
		{
			var router = new Router(8080);
			router
				.If(request => { Console.WriteLine(request.Client.Client.RemoteEndPoint); return false; })
				.RespondWith(null)
				.If(request => true)
				.RespondWith(request =>
					request
						.Respond(404)
						.StreamText("<html><head><title>404</title></head><body><h1>404 Not Found</h1></body></html>", "text/html")
				);
			router.Start();
		}
	}
}