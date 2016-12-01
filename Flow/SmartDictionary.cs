using System.Collections;
using System.Collections.Generic;

namespace Flow
{
    /// <remarks>
    ///     <para>
    ///         Manages addition and removal automagically.
    ///     </para>
    ///     <para>
    ///         The default value of V is returned in case the base dictionary does not contain the key.
    ///         When the value is set and the value equals the default value of V, the key is removed.
    ///     </para>
    /// </remarks>
    public class SmartDictionary<TK, TV> : IEnumerable<KeyValuePair<TK, TV>>
    {
        internal Dictionary<TK, TV> Dict;

        /// <summary>
        ///     Constructs a new <see cref="SmartDictionary" />, using the <see cref="Dictionary" /> as base.
        /// </summary>
        /// <param name="dict">
        ///     A <see cref="Dictionary" /> to use as base. Changes are reflected in both in the <see cref="Dictionary" /> and the
        ///     <see cref="SmartDictionary" />.
        /// </param>
        /// <remarks>
        ///     Changes are reflected in both in the <see cref="Dictionary" /> and the <see cref="SmartDictionary" />.
        /// </remarks>
        public SmartDictionary(Dictionary<TK, TV> dict)
        {
            this.Dict = dict;
        }

        /// <summary>
        ///     Constructs a new empty <see cref="SmartDictionary" />.
        /// </summary>
        public SmartDictionary()
            : this(new Dictionary<TK, TV>())
        {
        }

        //// <value>
        /// Sets or unsets the value related to the key.
        /// </value>
        /// <summary>
        ///     The default value of V is returned in case the base dictionary does not contain the key.
        ///     When the value is set and the value equals the default value of V, the key is removed.
        /// </summary>
        public TV this[TK key]
        {
            get
            {
                TV value;
                if (Dict.TryGetValue(key, out value))
                    return value;
                return default(TV);
            }
            set
            {
                if (Dict.ContainsKey(key))
                {
                    if (IsNullOrDefault(value)) // Remove if it is the default value.			
                        Dict.Remove(key);
                    else
                        Dict[key] = value;
                }
                else
                {
                    if (!IsNullOrDefault(value))
                        Dict.Add(key, value);
                }
            }
        }

        /// <summary>
        ///     Returns an IEnumerable to iterate through the keys and values.
        /// </summary>
        public IEnumerator<KeyValuePair<TK, TV>> GetEnumerator()
        {
            return Dict.GetEnumerator();
        }

        /// <summary>
        ///     Returns an IEnumerable to iterate through the keys and values.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return Dict.GetEnumerator();
        }

        private static bool IsNullOrDefault<T>(T value)
        {
            return value == null || value.Equals(default(TV));
        }
    }
}