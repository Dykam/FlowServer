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
				Console.WriteLine("Connection incoming: [{0}]", myNumber);
				if (!request.StreamFile(file => File.Exists(getAbsoluteFilePath(file)), "HTTP/1.1")) {
					if(!Directory.Exists(getAbsoluteFilePath(request.Path))) return false;
					var builder = new StringBuilder();
					var files = Directory.GetFiles(getAbsoluteFilePath(request.Path));
					var dirs = Directory.GetDirectories(getAbsoluteFilePath(request.Path));
					builder.Append(String.Format("<html><head><title>{0} File{1} found, {2} Director{3} found</title></head><body><ul>", files.Length, files.Length == 1 ? "" : "s", dirs.Length, dirs.Length == 1 ? "y" : "ies"));
					dirs.Select(path => path + "/")
						.Concat(files)
						.Select((path, i) => String.Format(@"<li id=""file-{0}""><a href=""{1}"">{1}</a></li>", i, getRelativeFilePath(path)))
						.Aggregate(builder, (b, line) => { b.Append(line); return b; });
					builder.Append("</ul></body></html>");
					request.StreamText(file => true, "HTTP/1.1", builder.ToString(), "text/html");
				}
				Console.WriteLine("[{0}] Closed", myNumber);
				request.Dispose();
				return true;
			});
			router.Start();
		}

		static string getAbsoluteFilePath(string relative)
		{
			if (Path.DirectorySeparatorChar != '/')
				relative = relative.Replace('/', Path.DirectorySeparatorChar);
			if (!relative.StartsWith("\\"))
				relative = "\\" + relative;
			var absolute = Environment.CurrentDirectory + relative;
			return absolute;
		}
		static string getRelativeFilePath(string absolute)
		{
			return absolute.Substring(Environment.CurrentDirectory.Length).Replace('\\', '/');
		}
	}
}
