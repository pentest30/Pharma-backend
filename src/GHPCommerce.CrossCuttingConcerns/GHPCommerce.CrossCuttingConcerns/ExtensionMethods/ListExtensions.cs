using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace GHPCommerce.CrossCuttingConcerns.ExtensionMethods
{
    public static class ListExtensions
    {
        public static List<T> Combines<T>(this List<T> collection1, List<T> collection2)
        {
            collection1.AddRange(collection2);
            return collection1;
        }
        public static IList<T> Clone<T>(this IList<T> listToClone) where T : ICloneable
        {
            return listToClone.Select(item => (T)item.Clone()).ToList();
        }
        public static ICollection<T> Combines<T>(this ICollection<T> collection1, ICollection<T> collection2)
        {
            var list = new List<T>();
            list.AddRange(collection1);
            list.AddRange(collection2);
            return list;
        }
        public static IList<T> Paged<T>(this IList<T> source, int page, int pageSize)
        {
            return (IList<T>)source.Skip((page - 1) * pageSize).Take(pageSize);
        }
        public static IEnumerable<TSource> DistinctBy<TSource, TKey> (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }
        public static byte[] FormatToBytes<T>(this ICollection<T> collection)
        {
            var binFormatter = new BinaryFormatter();
            var mStream = new MemoryStream();
            binFormatter.Serialize(mStream, collection);

            //This gives you the byte array.
           return mStream.ToArray();
        }
    }
}
