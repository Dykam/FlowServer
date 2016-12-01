using System;
using System.Collections.Generic;

namespace Flow
{
    public partial class Router
    {
        /// <remarks>
        ///     Adds responders to the router.
        /// </remarks>
        public class ResponderAdder
        {
            private readonly LinkedList<Predicate<Request>> _conditions;
            private readonly Router _router;

            private bool _used;

            internal ResponderAdder(Router router, Predicate<Request> condition)
            {
                _used = false;

                _conditions = new LinkedList<Predicate<Request>>();
                _conditions.AddLast(condition);

                this._router = router;
            }

            /// <summary>
            ///     Adds another condition to which the responder may respond.
            /// </summary>
            /// <param name="condition">
            ///     A <see cref="Predicate" /> which determines if the responder should respond to this request.
            /// </param>
            /// <returns>
            ///     A <see cref="ResponderAdder" /> to add the responder or another condition.
            /// </returns>
            public ResponderAdder OrIf(Predicate<Request> condition)
            {
                UsedChecker();
                _conditions.AddLast(condition);
                return this;
            }

            /// <summary>
            ///     Adds the responder to the router.
            /// </summary>
            /// <param name="responder">
            ///     A <see cref="Action" /> to respond with.
            /// </param>
            /// <returns>
            ///     A <see cref="Router" /> to which more listeners can be added.
            /// </returns>
            public Router RespondWith(Action<Request> responder)
            {
                UsedChecker();

                _used = true;
                _router.Processors.Add(new Responder(responder, _conditions));

                return _router;
            }

            private void UsedChecker()
            {
                if (_used) throw new InvalidOperationException("You can only add a responder once.");
            }
        }
    }
}