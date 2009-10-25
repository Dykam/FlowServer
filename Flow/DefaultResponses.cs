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
		public static bool StreamFile(this Request request,
			Predicate<String> predicate,
			Func<string, string> absolutePathFetcher,
			string version)
		{
			if(!predicate(request.Path)) return false;
			try {
				string absolutePath = absolutePathFetcher(request.Path);
				using (FileStream fileStream = File.OpenRead(absolutePath))
				using (
					var responseStream =
						request
						.Respond(version, 200, fileStream.Length)
						.Finish()) {
					var buffer = new Byte[BufferSize];
					while (true) {
						fileStream.Read(buffer, 0, buffer.Length);
						responseStream.Write(buffer, 0, buffer.Length);
					}
				}
			}
			catch (FileNotFoundException) {
				request.Respond(version, 404, 0).Finish().Close();
			}
			return true;
		}

		public static bool StreamFile(this Request request, Predicate<String> predicate, string version)
		{
			return StreamFile(request, predicate, getAbsoluteFilePath, version);
		}

		public static void AddFileStreamer(this Router router,
			Predicate<String> predicate,
			Func<string, string> absolutePathFetcher,
			string version)
		{
			router.Add(request =>
			{
				return request.StreamFile(predicate, absolutePathFetcher, version);
			});
		}

		public static void AddFileStreamer(this Router router, Predicate<String> predicate, string version)
		{
			AddFileStreamer(router, predicate, getAbsoluteFilePath, version);
		}

		public static bool StreamText(this Request request,
			Predicate<String> predicate, string version,
			string text, string mime, Encoding encoding)
		{
			if(!predicate(request.Path)) return false;

			var bytes = encoding.GetBytes(text);
			var response =
					request
					.Respond(version, 200, bytes.Length)
					.Custom
					.Add("Content-Type", mime)
					.Finish();
			using(response)
			{
				response.Write(bytes, 0, bytes.Length);
			}
			return true;
		}

		public static bool StreamText(this Request request,
			Predicate<String> predicate, string version,
			string text, string mime)
		{
			return StreamText(request, predicate, version, text, mime, Encoding.UTF8);
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
