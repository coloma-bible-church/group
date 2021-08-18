namespace Group.WebApi.Models
{
    using System.Collections.Generic;

    public static class DictionaryExtensions
    {
        public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        where TValue : new()
        {
            if (dictionary.TryGetValue(key, out var value))
                return value;
            return dictionary[key] = new TValue();
        }
    }
}