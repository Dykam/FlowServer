using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Flow;
using Flow.Rest;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Text.RegularExpressions;
using System.Reflection;

namespace Flow.FileServer
{
	class Program
	{
		static EventWaitHandle logHandle;
		static Thread logThread;
		static Queue<LogPart> logs;
		static object logsLocker;

		static void Main(string[] args)
		{
			var router = new Router();
			var counter = 0;
			router.Add(request =>
			{
				request.Accept();
				var ip = ((IPEndPoint)request.Client.Client.RemoteEndPoint).Address;
				ThreadPool.QueueUserWorkItem(o =>
				{
					var myNumber = counter++;
					var log = new Logger(ip);
					log.Log(String.Format("[{0}] Opened with {1}", myNumber, ip));
					request.CompleteHeaders();
					foreach(var logLine in
						from header in request.HeaderLines
						select String.Format("[{0}] H| {1}", myNumber, header))
						log.Log(logLine);

					string content = request.Path + "\r\n" + "User agent: " + request.Headers.FirstOrDefault(header => header.Key.ToLower() == "user-agent");
					log.Log(String.Format("[{0}] Content sent: {1}", myNumber, content));
					request.StreamText(_ => true, content);
					log.Log(String.Format("[{0}] Closed", myNumber));

					log.Enqueue();
				});
			});
			router.Start();

			logs = new Queue<LogPart>();
			logsLocker = new object();

			logHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
			logThread = new Thread(Program.log);
			logThread.Start();
		}

		static void log()
		{
			var dir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
			while (true) {
				if (logHandle.WaitOne()) {
					LogPart part;
					while (true) {
						lock (logsLocker) {
							if (logs.Count == 0)
								break;
							part = logs.Dequeue();
						}
						var path = Path.Combine(dir, part.Address.ToString()) + ".txt";
						if(!File.Exists(path)) File.Create(path);
						using (var file = new FileStream(path, FileMode.Append, FileAccess.Write)) {
							using (var writer = (TextWriter)new StreamWriter(file)) {
								writer.Write(part.Message);
								writer.Flush();
								file.Flush();
							}
						}
					}
				}
			}
		}

		static string getAbsoluteFilePath(string relative)
		{
			if (Path.DirectorySeparatorChar != '/')
				relative = relative.Replace('/', Path.DirectorySeparatorChar);
			if (!relative.StartsWith("\\"))
				relative = "\\" + relative;
			var absolute = Environment.CurrentDirectory + relative;
			return absolute;
		}
		static string getRelativeFilePath(string absolute)
		{
			return absolute.Substring(Environment.CurrentDirectory.Length).Replace('\\', '/');
		}

		class LogPart
		{
			public string Message { get; private set; }
			public IPAddress Address { get; private set; }
			public LogPart(string message, IPAddress address)
			{
				Message = message;
				Address = address;
			}
		}
		class Logger
		{
			StringBuilder message;
			public IPAddress Address { get; private set; }
			public Logger(IPAddress address)
			{
				Address = address;
				message = new StringBuilder();
			}

			public void Log(string message)
			{
				this.message.AppendLine(message);
				Console.WriteLine(message);
			}

			public void Enqueue()
			{
				lock (logsLocker)
					logs.Enqueue(ToLogPart());
				logHandle.Set();
			}

			public LogPart ToLogPart()
			{
				return new LogPart(this.ToString(), Address);
			}

			public override string ToString()
			{
				return message.ToString();
			}
		}

	}}
