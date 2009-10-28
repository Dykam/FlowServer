using System;
using System.Net;
using System.Linq;

namespace Flow.FileServer
{
	class Program
	{
		static void Main(string[] args)
		{
			var router = new Router();
			router
				.AddTextStreamer(_ => true, "Foo");
			router.Start();
		}

	}}
