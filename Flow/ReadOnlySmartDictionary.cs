using System.Collections;
using System.Collections.Generic;

namespace Flow
{
    /// <remarks>
    ///     <para>
    ///         Manages non-existing keys automagically.
    ///     </para>
    ///     <para>
    ///         The default value of V is returned in case the base dictionary does not contain the key.
    ///     </para>
    /// </remarks>
    public class ReadOnlySmartDictionary<TK, TV> : IEnumerable<KeyValuePair<TK, TV>>
    {
        internal Dictionary<TK, TV> Dict;

        /// <summary>
        ///     Constructs a new <see cref="ReadOnlySmartDictionary" />.
        /// </summary>
        /// <param name="dict">
        ///     A <see cref="SmartDictionary" /> to use as a base. Changes are reflected in this instance.
        /// </param>
        public ReadOnlySmartDictionary(SmartDictionary<TK, TV> dict)
        {
            this.Dict = dict.Dict;
        }

        /// <summary>
        ///     Constructs a new <see cref="ReadOnlySmartDictionary" />.
        /// </summary>
        /// <param name="dict">
        ///     A <see cref="Dictionary" /> to use as a base. Changes are reflected in this instance.
        /// </param>
        public ReadOnlySmartDictionary(Dictionary<TK, TV> dict)
        {
            this.Dict = dict;
        }

        /// <value>
        ///     Sets or unsets the value related to the key.
        /// </value>
        public TV this[TK key]
        {
            get
            {
                TV value;
                if (Dict.TryGetValue(key, out value))
                    return value;
                return default(TV);
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
    }
}