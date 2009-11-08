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
				var defaultParsersAttribute =
					Attribute
					.GetCustomAttribute(method, typeof(RestDefaultParserAttribute), true)
					as RestDefaultParserAttribute;
				Func<string, object>[] defaultParsers = null;
				if (defaultParsersAttribute != null)
					defaultParsers = defaultParsersAttribute.Parsers;


				var attributes =
					Attribute
					.GetCustomAttributes(method, typeof(RestMethodAttribute), true)
					.Cast<RestMethodAttribute>()
					.ToArray();
				if (attributes.Length != 0) {
					Func<string, object>[] parsers = null;

					foreach (var attribute in attributes) {
						if (attribute.Parsers == null && parsers == null) {
							parsers = defaultParsers ?? fetchParsersByParameter(method).ToArray();
						}

						var matchers = (
								from resource in attribute
									.Pattern
									.Split('/')
									.Where(s => !string.IsNullOrEmpty(s))
								select patternToRegex(resource)
							).ToArray();

						var handler = new RestHandler(method, attribute.Method, attribute.Parsers ?? parsers, matchers);
						router
							.If(handler.Takes)
							.RespondWith(handler.Handle);
					}
				} else {
					var parsers = fetchParsersByParameter(method).ToArray();
					var matchers = fetchMatchersByParameter(method).ToArray();

					var handler = new RestHandler(method, RequestMethods.All, parsers, matchers);
					router
						.If(handler.Takes)
						.RespondWith(handler.Handle);
				}
			}
		}

		static Regex patternToRegex(string pattern)
		{
			return new Regex("^(" + pattern + ")$", RegexOptions.Compiled);
		}

		static IEnumerable<Func<string, object>> fetchParsersByParameter(MethodInfo method)
		{
			return
				from param in method.GetParameters().Skip(1)
				select getParser(param.ParameterType);
		}

		static IEnumerable<Regex> fetchMatchersByParameter(MethodInfo method)
		{
			return
				from param in method.GetParameters().Skip(1)
				let matcher = getMatcher(param.ParameterType)
				select patternToRegex(matcher);
		}
	}

	class RestHandler
	{
		Regex[] matchers;
		MethodInfo method;
		Func<string, object>[] parsers;
		RequestMethods requestMethods;

		public RestHandler(MethodInfo method, RequestMethods requestMethods, Func<string, object>[] parsers, Regex[] matchers)
		{
			this.method = method;
			this.parsers = parsers;
			this.matchers = matchers;
			this.requestMethods = requestMethods;
		}

		public bool Takes(Request request)
		{
			Console.WriteLine("With: {0} {1}", requestMethods, matchers.StreamToString("/"));
			Console.WriteLine("I test {0} {1}", request.Method, request.Path);
			if ((request.Method | requestMethods) != request.Method)
				return false;
			
			var parts = request.Path.Split('/').Where(s => !string.IsNullOrEmpty(s)).ToArray();
			if (parts.Length != matchers.Length)
				return false;


			for (int i = 0; i < parts.Length; i++) {
				if (!matchers[i].IsMatch(parts[i]))
					return false;
			}
			Console.WriteLine("Match!");
			return true;
		}

		public void Handle(Request request)
		{
			var parts = request.Path.Split('/').Where(s => !string.IsNullOrEmpty(s)).ToArray();
			var parameters = matchers.ZipSelect(parts, parsers, (matcher, part, parse) =>
			{			
				var match = matcher.Match(part);
				var capture = match.Captures[0].ToString();
				return parse(capture);
			});

			method.Invoke(null, new object[] { request }.Concat(parameters).ToArray());

		}
	}

	[global::System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
	sealed class RestDefaultParserAttribute : Attribute
	{
		public RestDefaultParserAttribute()
		{
		}

		public Func<string, object>[] Parsers { get; set; }
	}

	[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
	public sealed class RestMethodAttribute : Attribute
	{
		public RestMethodAttribute()
		{
			Method = RequestMethods.All;
		}

		public Func<string, object>[] Parsers { get; set; }

		public RequestMethods Method { get; set; }

		/// <summary>
		/// The pattern which to match and reflect the path. Simple regular expressions are allowed.
		/// </summary>
		public string Pattern { get; set; }
	}
}