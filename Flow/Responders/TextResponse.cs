/*   Copyright 2009 Dykam (kramieb@gmail.com)
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *   http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Flow.Responders
{
	/// <remarks>
	/// Contains extension methods to easy responding to a request.
	/// This class focusses on textual responding.
	/// </remarks>
	public static class TextResponse
	{
		/// <summary>
		/// Streams a string to the response.
		/// </summary>
		/// <param name="response">
		/// A <see cref="HeaderBuilder"/> to get the response from.
		/// </param>
		/// <param name="text">
		/// A <see cref="System.String"/> to stream.
		/// </param>
		/// <param name="mime">
		/// A <see cref="System.String"/> denoting the mime type of the streamed string. Defaults to "text/plain".
		/// </param>
		/// <param name="encoding">
		/// A <see cref="Encoding"/> representing the encoding to encode the string with. Defaults to <see cref="UTF8Encoding"/>.
		/// </param>
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
		
		/// <summary>
		/// Streams a string to the response.
		/// </summary>
		/// <param name="response">
		/// A <see cref="HeaderBuilder"/> to get the response from.
		/// </param>
		/// <param name="text">
		/// A <see cref="System.String"/> to stream.
		/// </param>
		/// <param name="mime">
		/// A <see cref="System.String"/> denoting the mime type of the streamed string. Defaults to "text/plain".
		/// </param>
		public static void StreamText(this HeaderBuilder response, string text, string mime)
		{
			StreamText(response, text, mime, Encoding.UTF8);
		}
		
		/// <summary>
		/// Streams a string to the response.
		/// </summary>
		/// <param name="response">
		/// A <see cref="HeaderBuilder"/> to get the response from.
		/// </param>
		/// <param name="text">
		/// A <see cref="System.String"/> to stream.
		/// </param>
		public static void StreamText(this HeaderBuilder response, string text)
		{
			StreamText(response, text, "text/plain");
		}
	}
}
