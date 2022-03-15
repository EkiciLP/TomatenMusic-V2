using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.Linq;

namespace TomatenMusic.Util
{
    static class CollectionUtil
    {
        public static void Shuffle<T>(this IList<T> list)
        {
            RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
            int n = list.Count;
            while (n > 1)
            {
                byte[] box = new byte[1];
                do provider.GetBytes(box);
                while (!(box[0] < n * (Byte.MaxValue / n)));
                int k = (box[0] % n);
                n--;
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        /* public static void Remove<T>(this IList<T> list, T item)
         {
             List<T> newList = new List<T>();
             bool done = false;
             foreach (var i in list)
             {
                 if (i.Equals(item) && !done)
                 {
                     done = true;
                     continue;
                 }

                 newList.Add(i);
             }

             list = newList;
         }*/



        public static class Arrays
        {
            public static IList<T> AsList<T>(params T[] source)
            {
                return source;
            }
        }
    }
}
