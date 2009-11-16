using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Flow
{
	public partial class Router
	{
		/// <remarks>
		/// Adds responders to the router.
		/// </remarks>
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

			/// <summary>
			/// Adds another condition to which the responder may respond.
			/// </summary>
			/// <param name="condition">
			/// A <see cref="Predicate"/> which determines if the responder should respond to this request.
			/// </param>
			/// <returns>
			/// A <see cref="ResponderAdder"/> to add the responder or another condition.
			/// </returns>
			public ResponderAdder OrIf(Predicate<Request> condition)
			{
				usedChecker();
				conditions.AddLast(condition);
				return this;
			}

			/// <summary>
			/// Adds the responder to the router.
			/// </summary>
			/// <param name="responder">
			/// A <see cref="Action"/> to respond with.
			/// </param>
			/// <returns>
			/// A <see cref="Router"/> to which more listeners can be added.
			/// </returns>
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
