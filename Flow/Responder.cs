using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Flow
{
	public partial class Router
	{
		protected class Responder
		{
			Action<Request> responder;
			IEnumerable<Predicate<Request>> conditions;

			public Responder(Action<Request> responder, IEnumerable<Predicate<Request>> conditions)
			{
				this.responder = responder;
				this.conditions = conditions;
			}

			public bool Respond(Request request)
			{
				if (conditions.Any(condition => condition(request))) {
					responder(request);
					return true;
				}
				return false;
			}
		}
	}
}
