using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Flow
{
	public partial class Router
	{
		public class ResponderAdder
		{
			LinkedList<Predicate<Request>> conditions;
			Router router;

			bool used;

			internal ResponderAdder(Router router, Predicate<Request> condition)
			{
				used = false;

				conditions = new LinkedList<Predicate<Request>>();
				conditions.AddLast(condition);

				this.router = router;
			}

			public ResponderAdder OrIf(Predicate<Request> condition)
			{
				usedChecker();
				conditions.AddLast(condition);
				return this;
			}

			public Router RespondWith(Action<Request> responder)
			{
				usedChecker();

				used = true;
				router.processors.Add(new Responder(responder, conditions));

				return router;
			}

			void usedChecker()
			{
				if (used) throw new InvalidOperationException("You can only add a responder once.");
			}
		}
	}
}
