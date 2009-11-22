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
﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Flow
{
	/// <remarks>
	/// <para>
	/// Manages addition and removal automagically.
	/// </para>
	/// <para>
	/// The default value of V is returned in case the base dictionary does not contain the key.
	/// When the value is set and the value equals the default value of V, the key is removed.
	/// </para>
	/// </remarks>
	public class SmartDictionary<K, V> : IEnumerable<KeyValuePair<K, V>>
	{
		internal Dictionary<K, V> dict;
		
		/// <summary>
		/// Constructs a new <see cref="SmartDictionary"/>, using the <see cref="Dictionary"/> as base.
		/// </summary>
		/// <param name="dict">
		/// A <see cref="Dictionary"/> to use as base. Changes are reflected in both in the <see cref="Dictionary"/> and the <see cref="SmartDictionary"/>.
		/// </param>
		/// <remarks>
		/// Changes are reflected in both in the <see cref="Dictionary"/> and the <see cref="SmartDictionary"/>.
		/// </remarks>
		public SmartDictionary(Dictionary<K, V> dict)
		{
			this.dict = dict;
		}

		/// <summary>
		/// Constructs a new empty <see cref="SmartDictionary"/>.
		/// </summary>
		public SmartDictionary()
			: this(new Dictionary<K, V>())
		{
		}
		
		//// <value>
		/// Sets or unsets the value related to the key.
		/// </value>
		/// <summary>
		/// The default value of V is returned in case the base dictionary does not contain the key.
		/// When the value is set and the value equals the default value of V, the key is removed.
		/// </summary>
		public V this[K key]
		{
			get
			{
				V value;
				if(dict.TryGetValue(key, out value))
					return value;
				else
					return default(V);
			}
			set
			{
				if (dict.ContainsKey(key)) {
					if(isNullOrDefault(value)) // Remove if it is the default value.			
						dict.Remove(key);
					else
						dict[key] = value;
				} else {
					if(!isNullOrDefault(value))
						dict.Add(key, value);
				}
			}
		}
		
		static bool isNullOrDefault<T>(T value)
		{
			return value == null || value.Equals(default(V));
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
