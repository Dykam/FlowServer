using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Flow.Default
{
	public static class FileResponse
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
