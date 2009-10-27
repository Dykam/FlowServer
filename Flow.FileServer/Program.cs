using System;
using System.Net;

namespace Flow.FileServer
{
	class Program
	{
		static void Main(string[] args)
		{
			var router = new Router();
			var counter = 0;
			var random = new Random();
			router.Start();
		}

	}}
