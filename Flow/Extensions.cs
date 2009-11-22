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

namespace Flow
{
	static class IEnumerableExtensions
	{
		public static IEnumerable<TOut> ZipSelect<TIn1, TIn2, TOut>(this IEnumerable<TIn1> source1, IEnumerable<TIn2> source2, Func<TIn1, TIn2, TOut> selector)
		{
			IEnumerator<TIn1> ator1 = source1.GetEnumerator();
			IEnumerator<TIn2> ator2 = source2.GetEnumerator();

			while (ator1.MoveNext() && ator2.MoveNext()) {
				yield return selector(ator1.Current, ator2.Current);
			}
		}

		public static IEnumerable<TOut> ZipSelect<TIn1, TIn2, TIn3, TOut>(this IEnumerable<TIn1> source1, IEnumerable<TIn2> source2, IEnumerable<TIn3> source3, Func<TIn1, TIn2, TIn3, TOut> selector)
		{
			IEnumerator<TIn1> ator1 = source1.GetEnumerator();
			IEnumerator<TIn2> ator2 = source2.GetEnumerator();
			IEnumerator<TIn3> ator3 = source3.GetEnumerator();

			while (ator1.MoveNext() && ator2.MoveNext() && ator3.MoveNext()) {
				yield return selector(ator1.Current, ator2.Current, ator3.Current);
			}
		}

		public static string StreamToString<T>(this IEnumerable<T> source, string divider)
		{
			var builder = new StringBuilder();
			foreach (var item in source) {
				builder.Append(item);
				builder.Append(divider);
			}
			if (builder.Length == 0)
				return "";
			builder.Remove(builder.Length - divider.Length, divider.Length);
			return builder.ToString();
		}

		public static IEnumerable<T> Branch<T>(this IEnumerable<T> source, Action<IEnumerable<T>> action)
		{
			action(source);
			return source;
		}

		public static RequestMethods AsRequestMethod(string method)
		{
			switch (method.ToUpper()) {
				case "GET":
					return RequestMethods.Get;
				case "POST":
					return RequestMethods.Post;
				case "PUT":
					return RequestMethods.Put;
				case "HEAD":
					return RequestMethods.Head;
				case "DELETE":
					return RequestMethods.Delete;
				case "TRACE":
					return RequestMethods.Trace;
				case "ÇONNECT":
					return RequestMethods.Connect;
				default:
					return RequestMethods.None;
			}
		}
	}
}
