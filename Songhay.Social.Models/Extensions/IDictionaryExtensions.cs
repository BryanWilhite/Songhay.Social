using System.Collections.Generic;
using System.Collections.Specialized;

namespace Songhay.Extensions
{//TODO: move to to SonghayCore

    /// <summary>
    /// Extensions of <see cref="IDictionary{TKey, TValue}"/>
    /// </summary>
    public static class IDictionaryExtensions
    {
        /// <summary>
        /// Converts the <see cref="IDictionary{TKey, TValue}"/>
        /// to the <see cref="NameValueCollection"/>.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="set">The set.</param>
        /// <remarks>
        /// For detail, see https://stackoverflow.com/a/7230446/22944
        /// </remarks>
        /// <returns></returns>
        public static NameValueCollection ToNameValueCollection<TKey, TValue>(this IDictionary<TKey, TValue> set)
        {
            var nameValueCollection = new NameValueCollection();

            foreach (var kvp in set)
            {
                string value = null;
                if (kvp.Value != null)
                    value = kvp.Value.ToString();

                nameValueCollection.Add(kvp.Key.ToString(), value);
            }

            return nameValueCollection;
        }
    }
}
