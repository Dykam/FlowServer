using System;

namespace Flow.Handlers
{
    /// <summary>
    ///     Decorates a method with the default Parsers and/or Pattern to use.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public sealed class RestDefaultAttribute : Attribute
    {
        /// <value>
        ///     The default parsers to turn the resource arguments into function parameters.
        /// </value>
        public Func<string, object>[] Parsers { get; set; }

        public string Pattern { get; set; }
    }
}