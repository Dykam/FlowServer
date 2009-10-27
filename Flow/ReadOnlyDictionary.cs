//
//  Copyright (C) 2009 Chris Howie
//

using System;
using System.Collections;
using System.Collections.Generic;

namespace Flow
{
	public class ReadOnlyDictionary<K, V> : IDictionary<K, V>
	{
		protected IDictionary<K, V> Dictionary { get; private set; }

		public ReadOnlyDictionary(IDictionary<K, V> dictionary)
		{
			Dictionary = dictionary;
		}

		#region IDictionary<K,V>
		public virtual V this[K key]
		{
			get { return Dictionary[key]; }
			set { throw new NotSupportedException(); }
		}

		public virtual ICollection<K> Keys
		{
			get { return Dictionary.Keys; }
		}

		public virtual ICollection<V> Values
		{
			get { return Dictionary.Values; }
		}

		void IDictionary<K, V>.Add(K key, V value)
		{
			throw new NotSupportedException();
		}

		public virtual bool ContainsKey(K key)
		{
			return Dictionary.ContainsKey(key);
		}

		bool IDictionary<K, V>.Remove(K key)
		{
			throw new NotSupportedException();
		}

		public virtual bool TryGetValue(K key, out V value)
		{
			return Dictionary.TryGetValue(key, out value);
		}
		#endregion

		#region ICollection<KeyValuePair<K,V>>
		public virtual int Count
		{
			get { return Dictionary.Count; }
		}

		public virtual bool IsReadOnly
		{
			get { return true; }
		}

		void ICollection<KeyValuePair<K, V>>.Add(KeyValuePair<K, V> item)
		{
			throw new NotSupportedException();
		}

		void ICollection<KeyValuePair<K, V>>.Clear()
		{
			throw new NotSupportedException();
		}

		public virtual bool Contains(KeyValuePair<K, V> item)
		{
			return Dictionary.Contains(item);
		}

		public virtual void CopyTo(System.Collections.Generic.KeyValuePair<K, V>[] array, int arrayIndex)
		{
			Dictionary.CopyTo(array, arrayIndex);
		}

		bool ICollection<KeyValuePair<K, V>>.Remove(KeyValuePair<K, V> item)
		{
			throw new NotSupportedException();
		}
		#endregion

		#region IEnumerator
		public virtual IEnumerator<KeyValuePair<K, V>> GetEnumerator()
		{
			return Dictionary.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return Dictionary.GetEnumerator();
		}
		#endregion
	}
}
