using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Flow.Responders
{
	public static class TextResponse
	{
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
	}
}
