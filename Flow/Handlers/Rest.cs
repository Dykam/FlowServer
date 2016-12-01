using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Flow.Handlers
{
    /// <remarks>
    ///     Contains extension methods to more easily add conditional listening with a specialized responder.
    ///     This class focusses on following the Rest principle.
    /// </remarks>
    public static class Rest
    {
        /// <summary>
        ///     Adds a responder, and detects on what requests to respond.
        /// </summary>
        /// <param name="router">
        ///     A <see cref="Router" /> to add the responder to.
        /// </param>
        /// <param name="responder">
        ///     A <see cref="Action" /> to respond with.
        /// </param>
        /// <returns>
        ///     A <see cref="Router" /> to add more responders in flow.
        /// </returns>
        public static Router AddRest<T1, T2, T3>(this Router router, Action<Request, T1, T2, T3> responder)
        {
            router.Add(responder);
            return router;
        }

        /// <summary>
        ///     Adds a responder, and detects on what requests to respond.
        /// </summary>
        /// <param name="router">
        ///     A <see cref="Router" /> to add the responder to.
        /// </param>
        /// <param name="responder">
        ///     A <see cref="Action" /> to respond with.
        /// </param>
        /// <returns>
        ///     A <see cref="Router" /> to add more responders in flow.
        /// </returns>
        public static Router AddRest<T1, T2>(this Router router, Action<Request, T1, T2> responder)
        {
            router.Add(responder);
            return router;
        }

        /// <summary>
        ///     Adds a responder, and detects on what requests to respond.
        /// </summary>
        /// <param name="router">
        ///     A <see cref="Router" /> to add the responder to.
        /// </param>
        /// <param name="responder">
        ///     A <see cref="Action" /> to respond with.
        /// </param>
        /// <returns>
        ///     A <see cref="Router" /> to add more responders in flow.
        /// </returns>
        public static Router AddRest<T1>(this Router router, Action<Request, T1> responder)
        {
            router.Add(responder);
            return router;
        }

        /// <summary>
        ///     Adds a responder, and detects on what requests to respond.
        /// </summary>
        /// <param name="router">
        ///     A <see cref="Router" /> to add the responder to.
        /// </param>
        /// <param name="responder">
        ///     A <see cref="Action" /> to respond with.
        /// </param>
        /// <returns>
        ///     A <see cref="Router" /> to add more responders in flow.
        /// </returns>
        public static Router AddRest(this Router router, Delegate responder)
        {
            router.Add(responder);
            return router;
        }

        private static string GetMatcher(Type type)
        {
            if (type == typeof (int))
            {
                return "-?[0-9]{0, 21}";
            }
            if (type == typeof (uint))
            {
                return "[0-9]{1, 22}";
            }
            if (type == typeof (long))
            {
                return "-?[0-9]{0, 43}";
            }
            if (type == typeof (ulong))
            {
                return "[0-9]{1, 44}";
            }
            if (type == typeof (short))
            {
                return "-?[0-9]{0, 10}";
            }
            if (type == typeof (ushort))
            {
                return "[0-9]{1, 11}";
            }
            return ".*";
        }

        private static Func<string, object> GetParser(Type type)
        {
            if (type == typeof (int))
            {
                return s => int.Parse(s);
            }
            if (type == typeof (uint))
            {
                return s => uint.Parse(s);
            }
            if (type == typeof (long))
            {
                return s => long.Parse(s);
            }
            if (type == typeof (ulong))
            {
                return s => ulong.Parse(s);
            }
            if (type == typeof (short))
            {
                return s => short.Parse(s);
            }
            if (type == typeof (ushort))
            {
                return s => ushort.Parse(s);
            }
            return s => s;
        }

        private static void Add(this Router router, Delegate responder)
        {
            foreach (var method in responder.GetInvocationList().Select(del => del.Method))
            {
                var defaultAttribute =
                    Attribute
                        .GetCustomAttribute(method, typeof (RestDefaultAttribute), true)
                        as RestDefaultAttribute;
                Func<string, object>[] defaultParsers = null;
                Regex[] defaultMatchers = null;
                if (defaultAttribute != null)
                {
                    defaultParsers = defaultAttribute.Parsers;
                }

                var attributes = FetchAttributes(method).ToArray();

                if (attributes.Length != 0)
                {
                    Func<string, object>[] parsers = null;
                    Regex[] matchers = null;

                    foreach (var attribute in attributes)
                    {
                        if (attribute.Parsers == null && parsers == null)
                        {
                            parsers = defaultParsers ?? FetchParsersByParameter(method).ToArray();
                        }
                        if (attribute.Pattern == null && matchers == null)
                        {
                            matchers = defaultMatchers ?? FetchMatchersByParameter(method).ToArray();
                        }

                        Regex[] attributeMatchers = null;
                        if (attribute.Pattern != null)
                            attributeMatchers = PatternsToRegex(attribute.Pattern).ToArray();

                        var handler = new RestHandler(method, attribute.Method, attribute.Parsers ?? parsers,
                            attributeMatchers ?? matchers);
                        router
                            .If(handler.Takes)
                            .RespondWith(handler.Handle);
                    }
                }
                else
                {
                    var parsers = FetchParsersByParameter(method).ToArray();
                    var matchers = FetchMatchersByParameter(method).ToArray();

                    var handler = new RestHandler(method, RequestMethods.All, parsers, matchers);
                    router
                        .If(handler.Takes)
                        .RespondWith(handler.Handle);
                }
            }
        }

        private static IEnumerable<RestMethodAttribute> FetchAttributes(MethodInfo method)
        {
            var hasDefaultValue = false;
            var defaultValuesPattern =
                method
                    .GetParameters()
                    .Skip(1)
                    .Select(
                        param =>
                            (hasDefaultValue |= (param.Attributes & ParameterAttributes.HasDefault) != 0)
                                ? param.DefaultValue.ToString()
                                : ".*")
                    .Aggregate(new StringBuilder(), (builder, str) => builder.Append("/").Append(str))
                    .ToString();

            if (hasDefaultValue)
            {
                return
                    Attribute
                        .GetCustomAttributes(method, typeof (RestMethodAttribute), true)
                        .Cast<RestMethodAttribute>()
                        .Concat(new RestMethodAttribute {Pattern = defaultValuesPattern});
            }
            return
                Attribute
                    .GetCustomAttributes(method, typeof (RestMethodAttribute), true)
                    .Cast<RestMethodAttribute>();
        }

        private static IEnumerable<Regex> PatternsToRegex(string pattern)
        {
            return
                from resource in pattern.Split('/')
                where !string.IsNullOrEmpty(resource)
                select PatternToRegex(resource);
        }

        private static Regex PatternToRegex(string pattern)
        {
            return new Regex("^(" + pattern + ")$", RegexOptions.Compiled);
        }

        private static IEnumerable<Func<string, object>> FetchParsersByParameter(MethodInfo method)
        {
            return
                from param in method.GetParameters().Skip(1)
                select GetParser(param.ParameterType);
        }

        private static IEnumerable<Regex> FetchMatchersByParameter(MethodInfo method)
        {
            return
                from param in method.GetParameters().Skip(1)
                let matcher = GetMatcher(param.ParameterType)
                select PatternToRegex(matcher);
        }
    }

    internal class RestHandler
    {
        private readonly Regex[] _matchers;
        private readonly MethodInfo _method;
        private readonly Func<string, object>[] _parsers;
        private readonly RequestMethods _requestMethods;

        public RestHandler(MethodInfo method, RequestMethods requestMethods, Func<string, object>[] parsers,
            Regex[] matchers)
        {
            this._method = method;
            this._parsers = parsers;
            this._matchers = matchers;
            this._requestMethods = requestMethods;
        }

        public bool Takes(Request request)
        {
            if ((request.Method & _requestMethods) != request.Method)
                return false;

            var parts = request.Path.Split('/').Where(s => !string.IsNullOrEmpty(s)).ToArray();
            if (parts.Length != _matchers.Length)
                return false;


            for (var i = 0; i < parts.Length; i++)
            {
                if (!_matchers[i].IsMatch(parts[i]))
                    return false;
            }
            return true;
        }

        public void Handle(Request request)
        {
            var parts = request.Path.Split('/').Where(s => !string.IsNullOrEmpty(s)).ToArray();
            var parameters = _matchers.ZipSelect(parts, _parsers, (matcher, part, parse) =>
            {
                var match = matcher.Match(part);
                var capture = match.Captures[0].ToString();
                return parse(capture);
            }).ToArray();

            _method.Invoke(null, new object[] {request}.Concat(parameters).ToArray());
        }
    }
}