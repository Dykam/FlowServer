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
		const int CharBufferSize = 2048;
		public static void StreamFile(this Request request,
			Predicate<String> predicate,
			Func<string, string> absolutePathFetcher, Func<string, string> mimeFetcher)
		{
			if(!predicate(request.Path)) return;
			try {
				string absolutePath = absolutePathFetcher(request.Path);
				using (FileStream fileStream = File.OpenRead(absolutePath))
				using (
					var responseStream =
						request
						.Accept(200)
						.Add("Content-Type", mimeFetcher(absolutePath))
						.Add("Content-Lenght", fileStream.Length.ToString())
						.Finish()) {
					var buffer = new Byte[BufferSize];
					try {
						int bytesRead = 0;
						do {
							bytesRead = fileStream.Read(buffer, 0, buffer.Length);
							responseStream.Write(buffer, 0, bytesRead);
						} while (bytesRead != 0 && request.Client.Connected);
					}
					catch (Exception) { }
				}
			}
			catch (FileNotFoundException) {
				request.Accept(404).Finish().Close();
			}
		}

		public static void StreamFile(this Request request, Predicate<String> predicate, Func<string, string> mimeFetcher)
		{
			StreamFile(request, predicate, getAbsoluteFilePath, mimeFetcher);
		}

		public static void AddFileStreamer(this Router router,
			Predicate<String> predicate,
			Func<string, string> absolutePathFetcher,
			Func<string, string> mimeFetcher)
		{
			router.Add(request =>
			{
				request
					.StreamFile(predicate, absolutePathFetcher, mimeFetcher);
			});
		}

		public static void AddFileStreamer(this Router router,
			Predicate<String> predicate,
			Func<string, string> mimeFetcher)
		{
			AddFileStreamer(router, predicate, getAbsoluteFilePath, mimeFetcher);
		}

		public static void StreamText(this Request request,	Predicate<String> predicate, string text, string mime, Encoding encoding)
		{
			if(!predicate(request.Path)) return;

			var bytes = encoding.GetBytes(text);
			var response =
					request
					.Accept(200)
					.Add("Content-Type", mime)
					.Add("Content-Length", bytes.Length.ToString())
					.Finish();
			using(response)
			{
				response.Write(bytes, 0, bytes.Length);
			}
		}

		public static void StreamText(this Request request, Predicate<String> predicate, Func<Request, string> text, string mime, Encoding encoding)
		{
			StreamText(request, predicate, text(request), mime, encoding);
		}

		public static void StreamText(this Request request,	Predicate<String> predicate, string text, string mime)
		{
			StreamText(request, predicate, text, mime, Encoding.UTF8);
		}

		public static void StreamText(this Request request, Predicate<String> predicate, Func<Request, string> text, string mime)
		{
			StreamText(request, predicate, text(request), mime, Encoding.UTF8);
		}

		public static void StreamText(this Request request, Predicate<String> predicate, string text)
		{
			StreamText(request, predicate, text, "text/plain");
		}

		public static void StreamText(this Request request, Predicate<String> predicate, Func<Request, string> text)
		{
			StreamText(request, predicate, text(request), "text/plain");
		}

		public static void AddTextStreamer(this Router router, Predicate<String> predicate, string text, string mime, Encoding encoding)
		{
			router.Add(request =>
			{
				request
					.StreamText(predicate, text, mime, encoding);
			});
		}

		public static void AddTextStreamer(this Router router, Predicate<String> predicate, string text, string mime)
		{
			AddTextStreamer(router, predicate, text, mime, Encoding.UTF8);
		}

		public static void AddTextStreamer(this Router router, Predicate<String> predicate, string text)
		{
			AddTextStreamer(router, predicate, text, "text/plain", Encoding.UTF8);
		}

		public static void AddTextStreamer(this Router router, Predicate<String> predicate, Func<Request, string> text, string mime, Encoding encoding)
		{
			router.Add(request =>
			{
				request
					.StreamText(predicate, text(request), mime, encoding);
			});
		}

		public static void AddTextStreamer(this Router router, Predicate<String> predicate, Func<Request, string> text, string mime)
		{
			AddTextStreamer(router, predicate, text, mime, Encoding.UTF8);
		}

		public static void AddTextStreamer(this Router router, Predicate<String> predicate, Func<Request, string> text)
		{
			AddTextStreamer(router, predicate, text, "text/plain", Encoding.UTF8);
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
	}
}
