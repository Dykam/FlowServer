using System;
using System.Collections.Generic;
using System.IO;

namespace Flow
{
    /// <remarks>
    ///     Used add and write headers to the response.
    /// </remarks>
    public class HeaderBuilder : IDisposable
    {
        internal Stream Target;
        internal TextWriter Writer;

        internal HeaderBuilder(Stream target)
        {
            Target = target;
            Writer = new StreamWriter(target);
        }

        /// <value>
        ///     Returns true if the request started to stream the body.
        /// </value>
        public bool Finished { get; private set; }

        private Exception FinishedException => new Exception("Headers are already written.");

        /// <summary>
        ///     Flushes and Disposes the underlying stream.
        /// </summary>
        public void Dispose()
        {
            if (!Finished) Finish();
            Writer.Dispose();
        }

        /// <summary>
        ///     Adds and streams an header.
        /// </summary>
        /// <param name="key">
        ///     A <see cref="System.String" /> denoting the key of the header to stream.
        /// </param>
        /// <param name="value">
        ///     A <see cref="System.String" /> denoting the value of the header to stream.
        /// </param>
        /// <returns>
        ///     This.
        /// </returns>
        public HeaderBuilder Add(string key, string value)
        {
            if (Finished)
                throw FinishedException;
            Writer.WriteLine("{0}: {1}", key, value);
            Writer.Flush();
            return this;
        }

        /// <summary>
        ///     Adds and streams headers.
        /// </summary>
        /// <param name="key">
        ///     A <see cref="IEnumerable" /> denoting the keys and values of the header to stream.
        /// </param>
        /// <returns>
        ///     This.
        /// </returns>
        public HeaderBuilder Add(IEnumerable<KeyValuePair<string, string>> headers)
        {
            if (Finished)
                throw FinishedException;
            foreach (var header in headers)
            {
                Add(header);
            }
            return this;
        }

        /// <summary>
        ///     Adds and streams an header.
        /// </summary>
        /// <param name="key">
        ///     A <see cref="KeyValuePair" /> denoting the key and value of the header to stream.
        /// </param>
        /// <returns>
        ///     This.
        /// </returns>
        public HeaderBuilder Add(KeyValuePair<string, string> header)
        {
            if (Finished)
                throw FinishedException;
            return Add(header.Key, header.Value);
        }

        /// <summary>
        ///     Finishes streaming the headers to the client.
        /// </summary>
        /// <returns>
        ///     A <see cref="Stream" /> to write the response body to.
        /// </returns>
        public Stream Finish()
        {
            if (Finished)
                throw FinishedException;
            Writer.WriteLine();
            Writer.Flush();
            Finished = true;
            return Target;
        }
    }
}