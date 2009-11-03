using System;
using System.Net;
using System.Linq;
using Flow.Default;
using System.Text;

namespace Flow.FileServer
{
	static class Program
	{
		static void Main(string[] args)
		{
			var router = new Router();
			router
				.If(request => true)
				.RespondWith(request =>
				{
					var address = (IPEndPoint)request.Client.Client.RemoteEndPoint;
					request
						.Respond(200)
						.StreamText(String.Format("It works! Oh, and I know your IP, I think it is this one: {0}", address));
				});
			router.Start();
		}
	}}
