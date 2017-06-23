using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace MonolithicExtensions.Portable
{
    public static class CollectionExtensions
    {
        private static readonly Random BaseRandom = new Random(); 
        private static readonly Object RandomLock = new Object();

        //Get a random generator that doesn't care about the time.
        public static Random GetRandomGenerator()
        {
            lock (RandomLock)
            {
                return new Random(BaseRandom.Next());
            }
        }

        /// <summary>
        /// Determine if two dictionaries are equivalent even if they're somehow out of order. Calls the default
        /// "Equals" function on all objects.
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="Y"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="dictionary2"></param>
        /// <returns></returns>
        public static bool IsEquivalentTo<K, Y>(this IDictionary<K,Y> dictionary, IDictionary<K,Y> dictionary2)
        {
            return dictionary.Keys.Count() == dictionary2.Keys.Count &&
                dictionary.Keys.All(key => dictionary2.ContainsKey(key) && dictionary2[key].Equals(dictionary[key]));
        }

        /// <summary>
        /// Determine if two lists are the same set (contain the same elements regardless of order)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="list2"></param>
        /// <returns></returns>
        public static bool IsEquivalentTo<T>(this IEnumerable<T> list, IEnumerable<T> list2)
        {
            return list.Count() == list2.Count() && list.All(x => list.Where(y => y.Equals(x)).Count() == list2.Where(y => y.Equals(x)).Count());
        }

        /// <summary>
        /// Performs an in-place "Knuth" shuffle on the given list (Knuth didn't really invent it)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        public static void Shuffle<T>(this IList<T> list)
        {
            var random = GetRandomGenerator();
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var swapPosition = random.Next(i + 1);
                var temp = list[i];
                list[i] = list[swapPosition];
                list[swapPosition] = temp;
            }
        }

        public enum SortByExcessOption
        {
            PlaceAtEnd,
            PlaceAtBeginning,
            LeaveOut
        }

        /// <summary>
        /// Produce a new list which is sorted like the given list. This function is not highly performant (O(n^2)), 
        /// but it gets the job done.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="original"></param>
        /// <param name="sortByList"></param>
        /// <returns></returns>
        public static IList<T> SortBy<T>(this IList<T> original, IList<T> sortByList, SortByExcessOption excessOption = SortByExcessOption.PlaceAtEnd) 
        {
            return original.SortByWithEqualityFunction(sortByList, (T x, T y) => x.Equals(y), excessOption);
        }

        /// <summary>
        /// Produce a new list which is sorted like the given list. Sorting equality is performed by the given function.
        /// This function has poor performance.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="original"></param>
        /// <param name="sortByList"></param>
        /// <param name="equalityFunction"></param>
        /// <param name="excessOption"></param>
        /// <returns></returns>
        public static IList<T> SortByWithEqualityFunction<T, V>(this IList<T> original, IList<V> sortByList, 
            Func<T,V,bool> equalityFunction, SortByExcessOption excessOption = SortByExcessOption.PlaceAtEnd)
        {

            //First, do the VERY inefficient sorted insertion into the new list (while removing the elements from the old one)
            var newList = new List<T>();
            var originalCopy = new List<T>(original);

            foreach(V element in sortByList)
            {
                try
                {
                    //If this fails, it triggers the try/catch and thus we don't add or remove the elements
                    var originalElement = originalCopy.First(x => equalityFunction(x, element));
                    newList.Add(originalElement);
                    originalCopy.Remove(originalElement);
                }
                catch { }
            }

            //Next, figure out what to do with the rest.
            if (originalCopy.Count > 0)
            {
                if (excessOption == SortByExcessOption.PlaceAtBeginning)
                {
                    var sortedElements = newList;
                    newList = originalCopy.ToList();
                    newList.AddRange(sortedElements);
                }
                else if (excessOption == SortByExcessOption.PlaceAtEnd)
                {
                    newList.AddRange(originalCopy);
                }
            }

            return newList;
        }

    } //End class CollectionExtensions
} //End namespace PortableExtensions.Monolithic
