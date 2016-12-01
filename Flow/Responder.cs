using System;
using System.Collections.Generic;
using System.Linq;

namespace Flow
{
    public partial class Router
    {
        protected class Responder
        {
            private readonly IEnumerable<Predicate<Request>> _conditions;
            private readonly Action<Request> _responder;

            public Responder(Action<Request> responder, IEnumerable<Predicate<Request>> conditions)
            {
                this._responder = responder;
                this._conditions = conditions;
            }

            public bool Respond(Request request)
            {
                if (_conditions.Any(condition => condition(request)))
                {
                    _responder(request);
                    return true;
                }
                return false;
            }
        }
    }
}