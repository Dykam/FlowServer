using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Flow.Handlers
{
	public static class Rest
	{
		public static Router AddRest<T1, T2, T3>(this Router router, Action<Request, T1, T2, T3> responder)
		{
			router.add(responder);
			return router;
		}
		public static Router AddRest<T1, T2>(this Router router, Action<Request, T1, T2> responder)
		{
			router.add(responder);
			return router;
		}
		public static Router AddRest<T1>(this Router router, Action<Request, T1> responder)
		{
			router.add(responder);
			return router;
		}
		public static Router AddRest(this Router router, Delegate responder)
		{
			router.add(responder);
			return router;
		}

		static string getMatcher(Type type)
		{
			if (type == typeof(int)) {
				return "-?[0-9]{0, 21}";
			} else if (type == typeof(uint)) {
				return "[0-9]{1, 22}";
			} else if (type == typeof(long)) {
				return "-?[0-9]{0, 43}";
			} else if (type == typeof(ulong)) {
				return "[0-9]{1, 44}";
			} else if (type == typeof(short)) {
				return "-?[0-9]{0, 10}";
			} else if (type == typeof(ushort)) {
				return "[0-9]{1, 11}";
			} else {
				return ".*";
			}
		}

		static Func<string, object> getParser(Type type)
		{
			if (type == typeof(int)) {
				return s => int.Parse(s);
			} else if (type == typeof(uint)) {
				return s => uint.Parse(s);
			} else if (type == typeof(long)) {
				return s => long.Parse(s);
			} else if (type == typeof(ulong)) {
				return s => ulong.Parse(s);
			} else if (type == typeof(short)) {
				return s => short.Parse(s);
			} else if (type == typeof(ushort)) {
				return s => ushort.Parse(s);
			} else {
				return s => s;
			}
		}

		static void add(this Router router, Delegate responder)
		{
			foreach (var method in responder.GetInvocationList().Select(del => del.Method)) {
				var typeProcessor = (
					from param in method.GetParameters().Skip(1)
					let parser = getParser(param.ParameterType)
					let matcher = getMatcher(param.ParameterType)
					select new
					{
						Parser = parser,
						Matcher = matcher
					}).ToArray();
				var parsers = (
					from processor in typeProcessor
					select processor.Parser
					).ToArray();
				var matchers = (
					 from processor in typeProcessor
					 select new Regex("(" + processor.Matcher + ")")
					 ).ToArray();
				var handler = new RestHandler(method, parsers, matchers);
				router
					.If(handler.Takes)
					.RespondWith(handler.Handle);
			}
		}
	}

	class RestHandler
	{
		Regex[] matchers;
		MethodInfo method;
		Func<string, object>[] parsers;

		public RestHandler(MethodInfo method, Func<string, object>[] parsers, Regex[] matchers)
		{
			this.method = method;
			this.parsers = parsers;
			this.matchers = matchers;
		}

		public bool Takes(Request request)
		{
			var parts = request.Path.Split('/').Where(s => !string.IsNullOrEmpty(s)).ToArray();
			if (parts.Length != matchers.Length)
				return false;

			for (int i = 0; i > parts.Length; i++) {
				if (!matchers[i].IsMatch(parts[i]))
					return false;
			}
			return true;
		}

		public void Handle(Request request)
		{
			var parts = request.Path.Split('/');
			var parameters = matchers.ZipSelect(parts, parsers, (matcher, part, parse) =>
			{
				Console.WriteLine("Testing {0} for {1}", request.Path, matchers.StreamToString(", "));				
				var match = matcher.Match(part);
				var capture = match.Captures[0].ToString();
				return parse(capture);
			});

			method.Invoke(null, new object[] { request }.Concat(parameters).ToArray());

		}
	}

	[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
	public sealed class RestMethodAttribute : Attribute
	{
		public RestMethodAttribute()
		{
			Method = RequestMethods.All;
		}

		public RequestMethods Method { get; set; }

		/// <summary>
		/// The pattern which to match and reflect the path. Simple regular expressions are allowed.
		/// </summary>
		public string Pattern { get; set; }
	}
}