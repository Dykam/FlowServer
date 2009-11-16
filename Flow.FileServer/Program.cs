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
				.RespondWith(null);
			router.Start();
		}
	}
}