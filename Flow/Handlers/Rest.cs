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

		static void add(this Router router, Delegate responder)
		{
			foreach (var method in responder.GetInvocationList().Select(del => del.Method)) {
				var attributes = method.fetchAttributes();
				IEnumerator<RestMethodAttribute> enumer = attributes.GetEnumerator();
				var regexes = new List<Regex>();
				if (!enumer.MoveNext()) {
					regexes.Add(method.fetchPathMatcher());
				} else {
					do {
						var regex = enumer.Current.Pattern.Replace("/", ")/(");
						if (regex.EndsWith("("))
							regex = regex.Substring(0, regex.Length - 1);
						if (regex.StartsWith(")"))
							regex = regex.Substring(1);
					} while (enumer.MoveNext());
				}
			}
		}

		static Regex fetchPathMatcher(this MethodInfo responder)
		{
			var parts =
				responder
				.GetParameters()
				.Select(param => (param.DefaultValue ?? param.fetchPartMatcher()).ToString());
				
			var builder =
				parts
				.Aggregate(new StringBuilder("/("), (b, s) => b.Append(s).Append(")/("));

			return
				new Regex(
					builder
					.Remove(builder.Length - 2, 2)
					.ToString()
				);
		}

		static string fetchPartMatcher(this ParameterInfo info)
		{
			switch (info.ParameterType) {
				case typeof(int):
					return "-?[0-9]{1, 21}";
				case typeof(uint):
					return "[0-9]{1, 22}";
				case typeof(long):
					return "-?[0-9]{1, 43}";
				case typeof(ulong):
					return "[0-9]{1, 44}";
				case typeof(short):
					return "-?[0-9]{1, 10}";
				case typeof(ushort):
					return "[0-9]{1, 11}";
				case typeof(string):
				default:
					return ".*";
			}
		}

		static IEnumerable<RestMethodAttribute> fetchAttributes(this MethodInfo responder)
		{
			return responder.GetCustomAttributes(typeof(RestMethodAttribute), true).Cast<RestMethodAttribute>();
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

	public enum RequestMethods
	{
		None = 0,
		Head = 1 << 0,
		Get = 1 << 1,
		Post = 1 << 2,
		Put = 1 << 3,
		Delete = 1 << 4,
		Trace = 1 << 5,
		Options = 1 << 6,
		Connect = 1 << 7,
		All = Head | Get | Post | Put | Delete | Trace | Options | Connect
	}
}