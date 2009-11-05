using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

		}

		static IEnumerable<RestMethodAttribute> fetchAttributes(this Delegate responder)
		{
			return responder.Method.GetCustomAttributes(typeof(RestMethodAttribute), true).Cast<RestMethodAttribute>();
		}
	}

	[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
	public sealed class RestMethodAttribute : Attribute
	{
		public RestMethodAttribute()
		{
			Methods = RequestMethod.AllCommon;
		}

		public RequestMethod[] Methods { get; set; }

		/// <summary>
		/// The pattern which to match and reflect the path. Simple regular expressions are allowed.
		/// </summary>
		public string PathPattern { get; set; }
	}

	public class RequestMethod
	{
		string method;

		internal RequestMethod(string method)
		{
			Method = method;
		}

		public string Method
		{
			get
			{
				return method;
			}
			set
			{
				method = value.ToUpper();
			}
		}

		public static implicit operator RequestMethod(string method)
		{
			return new RequestMethod(method);
		}

		public static implicit operator string(RequestMethod method)
		{
			return method.Method;
		}

		public static implicit operator RequestMethod[](RequestMethod method)
		{
			return new[] { method };
		}

		public static bool operator ==(RequestMethod a, RequestMethod b)
		{
			return a.method == b.method;
		}

		public static bool operator ==(string a, RequestMethod b)
		{
			return a.ToUpper() == b.method;
		}

		public static bool operator ==(RequestMethod a, string b)
		{
			return b == a;
		}

		public static bool operator !=(RequestMethod a, RequestMethod b)
		{
			return !(a == b);
		}

		public static bool operator !=(string a, RequestMethod b)
		{
			return !(a == b);
		}

		public static bool operator !=(RequestMethod a, string b)
		{
			return !(a == b);
		}

		static RequestMethod()
		{
			Head = "Head";
			Get = "Get";
			Post = "Post";
			Put = "Put";
			Delete = "Delete";
			Trace = "Trace";
			Options = "Options";
			Connect = "Connect";

			AllCommon = new [] { Head, Get, Post, Put, Delete, Trace, Options, Connect };
		}

		public static RequestMethod Head { get; private set; }
		public static RequestMethod Get { get; private set; }
		public static RequestMethod Post { get; private set; }
		public static RequestMethod Put { get; private set; }
		public static RequestMethod Delete { get; private set; }
		public static RequestMethod Trace { get; private set; }
		public static RequestMethod Options { get; private set; }
		public static RequestMethod Connect { get; private set; }
		public static RequestMethod[] AllCommon { get; private set; }
	}
}