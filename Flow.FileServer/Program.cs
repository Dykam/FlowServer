using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Flow;
using System.IO;

namespace Flow.FileServer
{
	class Program
	{
		static void Main(string[] args)
		{
			var router = new Router();
			var counter = 0;
			router.Add(request =>
			{
				var myNumber = counter++;
				Console.WriteLine("Connection incoming.");
				request.CompleteHeaders();
				foreach(var header in request.Headers) {
					Console.WriteLine("[{0}] {1}: {2}", myNumber, header.Key, header.Value);
				}

				var reader = (TextReader) new StreamReader(request.Body);
				var line = reader.ReadLine();
				while (!String.IsNullOrEmpty(line)) {
					line = reader.ReadLine();
					Console.WriteLine("[{0}] {1}", myNumber, line);
				}
				writer.Flush();
				request.Dispose();
				Console.WriteLine("[{0}] Closed", myNumber);
				return true;
			});
			router.Start();
		}
	}
}
