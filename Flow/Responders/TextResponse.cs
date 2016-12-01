using System.Text;

namespace Flow.Responders
{
    /// <remarks>
    ///     Contains extension methods to easy responding to a request.
    ///     This class focusses on textual responding.
    /// </remarks>
    public static class TextResponse
    {
        /// <summary>
        ///     Streams a string to the response.
        /// </summary>
        /// <param name="response">
        ///     A <see cref="HeaderBuilder" /> to get the response from.
        /// </param>
        /// <param name="text">
        ///     A <see cref="System.String" /> to stream.
        /// </param>
        /// <param name="mime">
        ///     A <see cref="System.String" /> denoting the mime type of the streamed string. Defaults to "text/plain".
        /// </param>
        /// <param name="encoding">
        ///     A <see cref="Encoding" /> representing the encoding to encode the string with. Defaults to
        ///     <see cref="UTF8Encoding" />.
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
        ///     Streams a string to the response.
        /// </summary>
        /// <param name="response">
        ///     A <see cref="HeaderBuilder" /> to get the response from.
        /// </param>
        /// <param name="text">
        ///     A <see cref="System.String" /> to stream.
        /// </param>
        /// <param name="mime">
        ///     A <see cref="System.String" /> denoting the mime type of the streamed string. Defaults to "text/plain".
        /// </param>
        public static void StreamText(this HeaderBuilder response, string text, string mime = "text/plain")
        {
            StreamText(response, text, mime, Encoding.UTF8);
        }
    }
}