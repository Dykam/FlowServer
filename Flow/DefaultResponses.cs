using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Flow
{
	public static class DefaultResponses
	{
		const int BufferSize = 4096;
		public static void StreamFile(this HeaderBuilder response, string file, string mime)
		{
			using (FileStream fileStream = File.OpenRead(file)) {
				var responseStream =
					response
					.Add("Content-Type", mime)
					.Add("Content-Lenght", fileStream.Length.ToString())
					.Finish();
				using (responseStream) {
					var buffer = new Byte[BufferSize];
					int bytesRead = -1;
					while (bytesRead != 0 && responseStream.CanWrite && fileStream.CanRead) {
						bytesRead = fileStream.Read(buffer, 0, buffer.Length);
						responseStream.Write(buffer, 0, bytesRead);
					}
				}
			}
		}

		public static Router AddFileStreamer(this Router router,
			Predicate<Request> predicate,
			Func<Request, FileProperties> fetcher)
		{
			router.Add(request =>
			{
				if (predicate(request)) {
					var props = fetcher(request);
					request
						.Accept(200)
						.StreamFile(props.Path, props.Mime);
				}
			});
			return router;
		}

		public static void StreamText(this HeaderBuilder response, string text, string mime, Encoding encoding)
		{
			var bytes = encoding.GetBytes(text);
			var responseStream = 
					response
					.Add("Content-Type", mime)
					.Add("Content-Length", bytes.Length.ToString())
					.Finish();
			using (responseStream)
			{
				responseStream.Write(bytes, 0, bytes.Length);
			}
		}

		public static void StreamText(this HeaderBuilder response, string text, string mime)
		{
			StreamText(response, text, mime, Encoding.UTF8);
		}

		public static void StreamText(this HeaderBuilder response, string text)
		{
			StreamText(response, text, "text/plain");
		}

		public static Router AddTextStreamer(this Router router, Predicate<Request> predicate, string text, string mime, Encoding encoding)
		{
			router.Add(request =>
			{
				if(predicate(request)) {
					request
						.Accept(200)
						.StreamText(text, mime, encoding);
				}
			});
			return router;
		}

		public static Router AddTextStreamer(this Router router, Predicate<Request> predicate, string text, string mime)
		{
			AddTextStreamer(router, predicate, text, mime, Encoding.UTF8);
			return router;
		}

		public static Router AddTextStreamer(this Router router, Predicate<Request> predicate, string text)
		{
			AddTextStreamer(router, predicate, text, "text/plain", Encoding.UTF8);
			return router;
		}

		public static Router AddTextStreamer(this Router router, Predicate<Request> predicate, Func<Request, string> text, string mime, Encoding encoding)
		{
			router.Add(request =>
			{
				if (predicate(request)) {
					request
						.Accept(200)
						.StreamText(text(request), mime, encoding);
				}
			});
			return router;
		}

		public static Router AddTextStreamer(this Router router, Predicate<Request> predicate, Func<Request, string> text, string mime)
		{
			AddTextStreamer(router, predicate, text, mime, Encoding.UTF8);
			return router;
		}

		public static Router AddTextStreamer(this Router router, Predicate<Request> predicate, Func<Request, string> text)
		{
			AddTextStreamer(router, predicate, text, "text/plain", Encoding.UTF8);
			return router;
		}

		public class FileProperties
		{
			public string Mime { get; private set; }
			public string Path { get; private set; }
			public FileProperties(string mine, string path)
			{
				Mime = Mime;
				Path = path;
			}
		}
	}
}
