using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Flow;

namespace Flow.FileServer
{
	class Program
	{
		static void Main(string[] args)
		{
			var router = new Router();
			router.Add(request =>
			{
				return false;
			});
			router.Start();
		}
	}
}
