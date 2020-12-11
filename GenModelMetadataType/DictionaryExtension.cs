using System.Collections.Generic;

namespace GenModelMetadataType
{
    public static class DictionaryExtension
    {
        public static T GetValueOrDefault<T>(this IDictionary<string, T> dictionary, string key)
        {
            if (dictionary.TryGetValue(key, out T value))
            {
                return value;
            }

            return default;
        }
    }
}