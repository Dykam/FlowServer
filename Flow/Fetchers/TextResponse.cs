using System.IO;
using System.Text;

namespace Flow.Fetchers
{
    public static class TextFetchers
    {
        private const int BufferSize = 4096;

        public static string FetchText(this Request source, Encoding encoding)
        {
            int length;
            if (int.TryParse(source.Headers["Content-Length"], out length))
            {
                var buffer = new byte[length];
                var bytesRead = source.Body.Read(buffer, 0, buffer.Length);
                if (bytesRead == length)
                    return encoding.GetString(buffer);
            }

            int c;
            var builder = new StringBuilder();
            var reader = new StreamReader(source.Body);
            while (!reader.EndOfStream && (c = reader.Read()) != '\0')
            {
                builder.Append((char) c);
            }
            return builder.ToString();
        }

        public static string FetchText(this Request response)
        {
            return FetchText(response, Encoding.UTF8);
        }
    }
}