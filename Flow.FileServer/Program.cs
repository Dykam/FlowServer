using System;
using System.Net;
using System.Linq;
using Flow.Responders;
using System.Text;

namespace Flow.FileServer
{
	static class Program
	{
		static void Main(string[] args)
		{
			var router = new Router(8080);
			router
				.If(request => request.Path.Contains("ah"))
				.RespondWith(request =>
				{
					var address = (IPEndPoint)request.Client.Client.RemoteEndPoint;
					Console.WriteLine(address);
					request
						.Respond(200)
						.StreamText(String.Format(
@"It works! Oh, and I know your IP, I think it is this one: {0}
And you requested ""{1}"""
								                          , address, request.Path));
				});
			router.Start();
		}
	}
}
