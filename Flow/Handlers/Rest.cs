/*   Copyright 2009 Dykam (kramieb@gmail.com)
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *   http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

namespace Flow.Handlers
{
	/// <remarks>
	/// Contains extension methods to more easily add conditional listening with a specialized responder.
	/// This class focusses on following the Rest principle.
	/// </remarks>
	public static class Rest
	{
		/// <summary>
		/// Adds a responder, and detects on what requests to respond.
		/// </summary>
		/// <param name="router">
		/// A <see cref="Router"/> to add the responder to.
		/// </param>
		/// <param name="responder">
		/// A <see cref="Action"/> to respond with.
		/// </param>
		/// <returns>
		/// A <see cref="Router"/> to add more responders in flow.
		/// </returns>
		public static Router AddRest<T1, T2, T3>(this Router router, Action<Request, T1, T2, T3> responder)
		{
			router.add(responder);
			return router;
		}
		/// <summary>
		/// Adds a responder, and detects on what requests to respond.
		/// </summary>
		/// <param name="router">
		/// A <see cref="Router"/> to add the responder to.
		/// </param>
		/// <param name="responder">
		/// A <see cref="Action"/> to respond with.
		/// </param>
		/// <returns>
		/// A <see cref="Router"/> to add more responders in flow.
		/// </returns>
		public static Router AddRest<T1, T2>(this Router router, Action<Request, T1, T2> responder)
		{
			router.add(responder);
			return router;
		}
		/// <summary>
		/// Adds a responder, and detects on what requests to respond.
		/// </summary>
		/// <param name="router">
		/// A <see cref="Router"/> to add the responder to.
		/// </param>
		/// <param name="responder">
		/// A <see cref="Action"/> to respond with.
		/// </param>
		/// <returns>
		/// A <see cref="Router"/> to add more responders in flow.
		/// </returns>
		public static Router AddRest<T1>(this Router router, Action<Request, T1> responder)
		{
			router.add(responder);
			return router;
		}
		/// <summary>
		/// Adds a responder, and detects on what requests to respond.
		/// </summary>
		/// <param name="router">
		/// A <see cref="Router"/> to add the responder to.
		/// </param>
		/// <param name="responder">
		/// A <see cref="Action"/> to respond with.
		/// </param>
		/// <returns>
		/// A <see cref="Router"/> to add more responders in flow.
		/// </returns>
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
				var defaultAttribute =
					Attribute
					.GetCustomAttribute(method, typeof(RestDefaultAttribute), true)
					as RestDefaultAttribute;
				Func<string, object>[] defaultParsers = null;
				Regex[] defaultMatchers = null;
				if (defaultAttribute != null) {
					defaultParsers = defaultAttribute.Parsers;
				}

				var attributes = fetchAttributes(method).ToArray();
					
				if (attributes.Length != 0) {
					Func<string, object>[] parsers = null;
					Regex[] matchers = null;

					foreach (var attribute in attributes) {
						if (attribute.Parsers == null && parsers == null) {
							parsers = defaultParsers ?? fetchParsersByParameter(method).ToArray();
						}
						if (attribute.Pattern == null && matchers == null) {
							matchers = defaultMatchers ?? fetchMatchersByParameter(method).ToArray();
						}

						Regex[] attributeMatchers = null;
						if (attribute.Pattern != null)
							attributeMatchers = patternsToRegex(attribute.Pattern).ToArray();

						var handler = new RestHandler(method, attribute.Method, attribute.Parsers ?? parsers, attributeMatchers ?? matchers);
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
		
		static IEnumerable<RestMethodAttribute> fetchAttributes(MethodInfo method)
		{
			bool hasDefaultValue = false;
			var defaultValuesPattern =
				method
				.GetParameters()
				.Skip(1)
				.Select(param => Attribute.GetCustomAttribute(param, typeof(DefaultParameterValueAttribute)))
				.Cast<DefaultParameterValueAttribute>()
				.Select(attribute => (hasDefaultValue |= attribute != null) ? (string)attribute.Value : ".*")
				.Aggregate(new StringBuilder(), (builder, str) => builder.Append("/").Append(str))
				.ToString();
			Console.WriteLine(defaultValuesPattern);
			Console.WriteLine(hasDefaultValue);
			
			if(hasDefaultValue) {
				return
					Attribute
						.GetCustomAttributes(method, typeof(RestMethodAttribute), true)
						.Cast<RestMethodAttribute>()
						.Concat(new RestMethodAttribute() { Pattern = defaultValuesPattern });
			} else {
				return
					Attribute
						.GetCustomAttributes(method, typeof(RestMethodAttribute), true)
						.Cast<RestMethodAttribute>();
			}
		}

		static IEnumerable<Regex> patternsToRegex(string pattern) {
			return
				from resource in pattern.Split('/')
				where !string.IsNullOrEmpty(resource)
				select patternToRegex(resource);
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
			if ((request.Method & requestMethods) != request.Method)
				return false;
			
			var parts = request.Path.Split('/').Where(s => !string.IsNullOrEmpty(s)).ToArray();
			if (parts.Length != matchers.Length)
				return false;


			for (int i = 0; i < parts.Length; i++) {
				if (!matchers[i].IsMatch(parts[i]))
					return false;
			}
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
			}).ToArray();

			method.Invoke(null, new object[] { request }.Concat(parameters).ToArray());
		}
	}
}