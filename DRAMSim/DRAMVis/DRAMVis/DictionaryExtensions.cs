using System;
using System.Collections.Generic;


namespace DictionaryExtensions 
{
	public static class DictionaryExtensions 
	{
		public static void AddReplace<K, V>(this Dictionary<K, V> dictionary, K key, V value) 
		{
			if (dictionary == null) 
				throw new ArgumentNullException("dictionary");

			if (dictionary.ContainsKey(key))
				dictionary[key] = value;
			else
				dictionary.Add(key, value);
		}
	}
}
