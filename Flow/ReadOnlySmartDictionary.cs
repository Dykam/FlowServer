/*   Copyright 2009 Dykam (kramieb@gmail.com)
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *   http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using System.Collections;
using System.Collections.Generic;

namespace Flow
{
	/// <remarks>
	/// <para>
	/// Manages non-existing keys automagically.
	/// </para>
	/// <para>
	/// The default value of V is returned in case the base dictionary does not contain the key.
	/// </para>
	/// </remarks>
	public class ReadOnlySmartDictionary<K, V> : IEnumerable<KeyValuePair<K, V>>
	{
		internal Dictionary<K, V> dict;

		/// <summary>
		/// Constructs a new <see cref="ReadOnlySmartDictionary"/>.
		/// </summary>
		/// <param name="dict">
		/// A <see cref="SmartDictionary"/> to use as a base. Changes are reflected in this instance.
		/// </param>
		public ReadOnlySmartDictionary(SmartDictionary<K, V> dict)
		{
			this.dict = dict.dict;
		}

		/// <summary>
		/// Constructs a new <see cref="ReadOnlySmartDictionary"/>.
		/// </summary>
		/// <param name="dict">
		/// A <see cref="Dictionary"/> to use as a base. Changes are reflected in this instance.
		/// </param>
		public ReadOnlySmartDictionary(Dictionary<K, V> dict)
		{
			this.dict = dict;
		}

		/// <value>
		/// Sets or unsets the value related to the key.
		/// </value>
		public V this[K key]
		{
			get
			{
				V value;
				if (dict.TryGetValue(key, out value))
					return value;
				else
					return default(V);
			}
		}

		/// <summary>
		///Returns an IEnumerable to iterate through the keys and values. 
		/// </summary>
		public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
		{
			return dict.GetEnumerator();
		}
		
		/// <summary>
		///Returns an IEnumerable to iterate through the keys and values. 
		/// </summary>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return dict.GetEnumerator();
		}
	}
}
