using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Flow
{
	/// <remarks>
	/// Used add and write headers to the response.
	/// </remarks>
	public class HeaderBuilder : IDisposable
	{
		internal Stream Target;
		internal TextWriter Writer;

		/// <value>
		/// Returns true if the request started to stream the body.
		/// </value>
		public bool Finished { get; private set; }

		internal HeaderBuilder(Stream target)
		{
			this.Target = target;
			Writer = (TextWriter)new StreamWriter(target);
		}

		/// <summary>
		/// Adds and streams an header.
		/// </summary>
		/// <param name="key">
		/// A <see cref="System.String"/> denoting the key of the header to stream.
		/// </param>
		/// <param name="value">
		/// A <see cref="System.String"/> denoting the value of the header to stream.
		/// </param>
		/// <returns>
		/// This.
		/// </returns>
		public HeaderBuilder Add(string key, string value)
		{
			if (Finished)
				throw finishedException;
			Writer.WriteLine("{0}: {1}", key, value);
			Writer.Flush();
			return this;
		}

		/// <summary>
		/// Adds and streams headers.
		/// </summary>
		/// <param name="key">
		/// A <see cref="IEnumerable"/> denoting the keys and values of the header to stream.
		/// </param>
		/// <returns>
		/// This.
		/// </returns>
		public HeaderBuilder Add(IEnumerable<KeyValuePair<string, string>> headers)
		{
			if (Finished)
				throw finishedException;
			foreach (var header in headers) {
				Add(header);
			}
			return this;
		}
		
		/// <summary>
		/// Adds and streams an header.
		/// </summary>
		/// <param name="key">
		/// A <see cref="KeyValuePair"/> denoting the key and value of the header to stream.
		/// </param>
		/// <returns>
		/// This.
		/// </returns>
		public HeaderBuilder Add(KeyValuePair<string, string> header)
		{
			if (Finished)
				throw finishedException;
			return Add(header.Key, header.Value);
		}

		/// <summary>
		/// Finishes streaming the headers to the client.
		/// </summary>
		/// <returns>
		/// A <see cref="Stream"/> to write the response body to.
		/// </returns>
		public Stream Finish()
		{
			if (Finished)
				throw finishedException;
			Writer.WriteLine();
			Writer.Flush();
			Finished = true;
			return Target;
		}
		
		/// <summary>
		/// Flushes and Disposes the underlying stream.
		/// </summary>
		public void Dispose()
		{
			if (!Finished) Finish();
			Writer.Dispose();
		}

		Exception finishedException
		{
			get
			{
				return new Exception("Headers are already written.");
			}
		}
	}
}
