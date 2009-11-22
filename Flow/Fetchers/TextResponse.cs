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

namespace Flow.Fetchers
{
	public static class TextFetchers
	{
		const int BufferSize = 4096;
		public static string FetchText(this Request source, Encoding encoding)
		{
			int length;
			if(int.TryParse(source.Headers["Content-Length"], out length)) {
				var buffer = new Byte[length];
				var bytesRead = source.Body.Read(buffer, 0, buffer.Length);
				if (bytesRead == length)
					return encoding.GetString(buffer);
			}

			int c;
			var builder = new StringBuilder();
			var reader = new StreamReader(source.Body);
			while (!reader.EndOfStream && (c = reader.Read()) != '\0') {
				builder.Append((char)c);
			}
			return builder.ToString();
		}

		public static string FetchText(this Request response)
		{
			return FetchText(response, Encoding.UTF8);
		}
	}
}
