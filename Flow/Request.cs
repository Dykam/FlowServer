using System;
using System.IO;
using System.Net.Sockets;
using Flow.Properties;

namespace Flow
{
    /// <remarks>
    ///     Contains the incoming request.
    /// </remarks>
    public class Request : RequestInfo
    {
        internal Request(Router router, TcpClient newClient, int port, Func<int, string> statusMessageFetcher,
            string httpVersion)
            : base(router, newClient, port, statusMessageFetcher, httpVersion)
        {
        }

        internal Request(Router router, TcpClient newClient, int port, Func<int, string> statusMessageFetcher)
            : this(router, newClient, port, statusMessageFetcher, DefaultHtppVersion)
        {
        }

        internal Request(Router router, TcpClient newClient, int port, string httpVersion)
            : this(
                router, newClient, port, status => (string) Settings.Default["Status" + status.ToString()], httpVersion)
        {
        }

        internal Request(Router router, TcpClient newClient, int port)
            : this(router, newClient, port, DefaultHtppVersion)
        {
        }

        /// <value>
        ///     Returns true if there is already responded to the request.
        /// </value>
        public bool Used { get; private set; }

        /// <value>
        ///     The body of the request.
        /// </value>
        public ReadOnlyNetworkStream Body => BodyInternal;

        /// <summary>
        ///     Starts the response to the request.
        /// </summary>
        /// <param name="version">
        ///     A <see cref="System.String" /> denoting the HTTP version of the response.
        /// </param>
        /// <param name="status">
        ///     A <see cref="System.Int32" /> denoting the HTTP status code.
        /// </param>
        /// <param name="statusMessage">
        ///     A <see cref="System.String" />  denoting the human readable form of the status code.
        /// </param>
        /// <returns>
        ///     A <see cref="HeaderBuilder" /> to add the headers to return to the client.
        /// </returns>
        public HeaderBuilder Respond(string version, int status, string statusMessage)
        {
            CheckIfUsed();
            Used = true;

            var writer = (TextWriter) new StreamWriter(Stream);
            writer.WriteLine("{0} {1} {2}", version, status, statusMessage);
            writer.Flush();
            Response = new HeaderBuilder(new WriteOnlyStreamWrapper(Stream));
            return Response;
        }

        /// <summary>
        ///     Starts the response to the request.
        /// </summary>
        /// <param name="version">
        ///     A <see cref="System.String" /> denoting the HTTP version of the response.
        /// </param>
        /// <param name="status">
        ///     A <see cref="System.Int32" /> denoting the HTTP status code.
        /// </param>
        /// <returns>
        ///     A <see cref="HeaderBuilder" /> to add the headers to return to the client.
        /// </returns>
        public HeaderBuilder Respond(string version, int status)
        {
            return Respond(version, status, GetStatusMessage(status));
        }

        /// <summary>
        ///     Starts the response to the request.
        /// </summary>
        /// <param name="status">
        ///     A <see cref="System.Int32" /> denoting the HTTP status code.
        /// </param>
        /// <returns>
        ///     A <see cref="HeaderBuilder" /> to add the headers to return to the client.
        /// </returns>
        public HeaderBuilder Respond(int status)
        {
            return Respond(HttpVersion, status);
        }

        /// <summary>
        ///     Starts the response to the request.
        /// </summary>
        /// <param name="status">
        ///     A <see cref="System.Int32" /> denoting the HTTP status code.
        /// </param>
        /// <param name="statusMessage">
        ///     A <see cref="System.String" />  denoting the human readable form of the status code.
        /// </param>
        /// <returns>
        ///     A <see cref="HeaderBuilder" /> to add the headers to return to the client.
        /// </returns>
        public HeaderBuilder Respond(int status, string statusMessage)
        {
            return Respond(HttpVersion, status, statusMessage);
        }

        private void CheckIfUsed()
        {
            if (Used)
                throw new InvalidOperationException("You can not use a request twice.");
        }

        public override string ToString()
        {
            return string.Format("{0}: {1} {2} {3}.", Client.Client.RemoteEndPoint, Method, Path, Version);
        }
    }
}