using System.IO;

namespace Flow.Responders
{
    /// <remarks>
    ///     Contains extension methods to easy responding to a request.
    ///     This class focusses on file wise responding.
    /// </remarks>
    public static class FileResponse
    {
        private const int BufferSize = 4096;

        /// <summary>
        ///     Streams a file to the response.
        /// </summary>
        /// <param name="response">
        ///     A <see cref="HeaderBuilder" /> to get the response from.
        /// </param>
        /// <param name="file">
        ///     A <see cref="System.String" /> representing an absolute path pointing to the file to stream.
        /// </param>
        /// <param name="mime">
        ///     A <see cref="System.String" /> denoting the mime type of the streamed string. Defaults to
        ///     "application/octet-stream".
        /// </param>
        public static void StreamFile(this HeaderBuilder response, string file, string mime)
        {
            using (var fileStream = File.OpenRead(file))
            {
                var responseStream =
                    response
                        .Add("Content-Type", mime)
                        .Add("Content-Lenght", fileStream.Length.ToString())
                        .Add("Connection", "Close")
                        .Finish();
                using (responseStream)
                {
                    var buffer = new byte[BufferSize];
                    while (responseStream.CanWrite && fileStream.CanRead)
                    {
                        var bytesRead = fileStream.Read(buffer, 0, buffer.Length);
                        if (bytesRead == 0)
                            break;
                        responseStream.Write(buffer, 0, bytesRead);
                    }
                    responseStream.Flush();
                }
            }
        }

        /// <summary>
        ///     Streams a file to the response.
        /// </summary>
        /// <param name="response">
        ///     A <see cref="HeaderBuilder" /> to get the response from.
        /// </param>
        /// <param name="file">
        ///     A <see cref="System.String" /> representing an absolute path pointing to the file to stream.
        /// </param>
        public static void StreamFile(this HeaderBuilder response, string file)
        {
            StreamFile(response, file, "application/octet-stream");
        }
    }
}