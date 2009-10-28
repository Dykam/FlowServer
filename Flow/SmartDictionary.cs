using System;
using System.Collections;
using System.Collections.Generic;

namespace Flow
{
	public class SmartDictionary<K, V> : IEnumerable<KeyValuePair<K, V>>
	{
		internal Dictionary<K, V> dict;

		public SmartDictionary(Dictionary<K, V> dict)
		{
			this.dict = dict;
		}

		public SmartDictionary()
			: this(new Dictionary<K, V>())
		{
		}

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
					dict[key] = value;
				} else {
					dict.Add(key, value);
				}
			}
		}

		#region IEnumerable<KeyValuePair<K,V>> Members

		public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
		{
			return dict.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return dict.GetEnumerator();
		}

		#endregion
	}
}
