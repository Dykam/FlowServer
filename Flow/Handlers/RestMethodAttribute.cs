using System;

namespace Flow.Handlers
{
    /// <summary>
    ///     Decorates a method with properties denoting the REST resource to represent.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public sealed class RestMethodAttribute : Attribute
    {
        public RestMethodAttribute()
        {
            Method = RequestMethods.All;
        }

        /// <value>
        ///     The parsers to turn the resource arguments into function parameters.
        /// </value>
        public Func<string, object>[] Parsers { get; set; }

        //// <value>
        /// The RequestMethod to accept.
        /// </value>
        public RequestMethods Method { get; set; }

        /// <summary>
        ///     The pattern which to match and reflect the path. Simple regular expressions are allowed.
        /// </summary>
        public string Pattern { get; set; }
    }
}