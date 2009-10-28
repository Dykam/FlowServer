using System;
using System.Collections;
using System.Collections.Generic;

namespace Flow
{

	public class ReadOnlySmartDictionary<K, V> : IEnumerable<KeyValuePair<K, V>>
	{
		internal Dictionary<K, V> dict;

		public ReadOnlySmartDictionary(SmartDictionary<K, V> dict)
		{
			this.dict = dict.dict;
		}

		public ReadOnlySmartDictionary(Dictionary<K, V> dict)
		{
			this.dict = dict;
		}

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
