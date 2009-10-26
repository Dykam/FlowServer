using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Flow.Rest
{
	public class Rest
	{
		List<Rest> children;
		public Regex Expression { get; private set; }
		public Rest(Regex expression, params Rest[] children)
		{
			Expression = expression;
			this.children = new List<Rest>(children);
		}
	}
	public static class StaticRest
	{
		public static Rest AddRestListener(this Router router, Regex expression, params Rest[] children)
		{
			return new Rest(expression, children);
		}
	}
}
