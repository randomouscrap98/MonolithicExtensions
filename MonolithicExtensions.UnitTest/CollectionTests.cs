using Microsoft.VisualBasic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MonolithicExtensions.Portable;

namespace MonolithicExtensions.UnitTest
{
    /// <summary>
    /// Unit tests for testing the CollectionExtensions functionality.
    /// </summary>
    [TestClass()]
    public class CollectionsTest
    {
        public readonly List<int> SimpleList = new List<int>() { 3, 2, 6, 2, 7 };
        public readonly Dictionary<int, string> SimpleDictionary = new Dictionary<int, string>
        {
            {2, "heh two"},
            {22, "heh another two"},
            {3, "wow three so original"},
            {6, "skipped a few numbers there"},
            {7, "lucky"}
        };

        [TestMethod()]
        public void TestIsEquivalent_List()
        {
            List<int> myList = new List<int>(SimpleList);
            List<int> myList2 = new List<int>(SimpleList);

            myList2.Sort();

            Assert.IsFalse(myList.SequenceEqual(myList2));
            Assert.IsTrue(myList.IsEquivalentTo(myList2));

            myList.Add(7);
            Assert.IsFalse(myList.IsEquivalentTo(myList2));

            myList2.Add(2);
            Assert.IsFalse(myList.IsEquivalentTo(myList2));

        }

        [TestMethod()]
        public void TestIsEquivalent_Dictionary()
        {
            Dictionary<int, string> myDict = new Dictionary<int, string>(SimpleDictionary);
            SortedDictionary<int, string> myDict2 = new SortedDictionary<int, string>(SimpleDictionary);

            Assert.IsFalse(myDict.ToList().SequenceEqual(myDict2.ToList()));
            Assert.IsTrue(myDict.IsEquivalentTo(myDict2));

            myDict.Add(8, "gr8 b8 m8 r8 8/8");
            Assert.IsFalse(myDict.IsEquivalentTo(myDict2));

            myDict2.Add(8, "gr9 b9 m9 r9 9/9");
            Assert.IsFalse(myDict.IsEquivalentTo(myDict2));
        }

        [TestMethod()]
        public void TestMerge_Dictionary()
        {
            Dictionary<int, string> myDict = new Dictionary<int, string>(SimpleDictionary);
            Dictionary<int, string> mySilliness = new Dictionary<int, string>() { { 2, "don't break my two" } };

            Assert.IsTrue(myDict.MergeWithSelf(mySilliness) == 0);
            Assert.IsTrue(myDict.IsEquivalentTo(SimpleDictionary));

            Assert.IsTrue(myDict.MergeWithSelf(mySilliness, true) == 1);
            Assert.IsFalse(myDict.IsEquivalentTo(SimpleDictionary));
            Assert.IsTrue(myDict.Count == SimpleDictionary.Count);
            Assert.IsTrue(myDict[2] == mySilliness[2]);

            mySilliness.Add(99999999, "wow really big");
            mySilliness.Add(99999998, "wow almost really big");
            Assert.IsTrue(myDict.MergeWithSelf(mySilliness) == 2);
            Assert.IsFalse(myDict.IsEquivalentTo(SimpleDictionary));
            Assert.IsTrue(myDict.Count > SimpleDictionary.Count);
            Assert.IsTrue(myDict[99999999] == mySilliness[99999999]);
            Assert.IsTrue(myDict[99999998] == mySilliness[99999998]);
        }

        [TestMethod()]
        public void TestShuffle()
        {
            //Create an array of a hundred integers. Create a copy of it too.
            List<int> original = new List<int>();
            for (int i = 0; i <= 100; i++)
            {
                original.Add(i);
            }
            List<int> tester = new List<int>(original);
            List<int> testerPrevious = new List<int>(original);

            //Make sure they're the same before continuing
            Assert.IsTrue(original.SequenceEqual(tester));

            //Perform a few shuffles and make sure they're always different but that the collection is always the same
            for (int i = 0; i <= 100; i++)
            {
                tester.Shuffle();
                Assert.IsFalse(tester.SequenceEqual(original));
                Assert.IsFalse(tester.SequenceEqual(testerPrevious));
                Assert.IsTrue(tester.IsEquivalentTo(original));
                Assert.IsTrue(tester.IsEquivalentTo(testerPrevious));
                testerPrevious = new List<int>(tester);
            }
        }

        [TestMethod()]
        public void TestSortBy()
        {
            List<int> original = default(List<int>);
            List<int> sortedThang = default(List<int>);
            CollectionExtensions.SortByExcessOption sortOption = default(CollectionExtensions.SortByExcessOption);
            List<int> newList = default(List<int>);

            for (int i = 0; i <= 10; i++)
            {
                original = new List<int>(SimpleList);
                sortedThang = new List<int>(SimpleList);
                sortOption = CollectionExtensions.SortByExcessOption.PlaceAtEnd;
                sortedThang.Shuffle();

                if (i >= 9)
                {
                    sortOption = CollectionExtensions.SortByExcessOption.LeaveOut;
                }
                else if (i > 7)
                {
                    sortOption = CollectionExtensions.SortByExcessOption.PlaceAtBeginning;
                }

                if (i > 5)
                    original.Add(200 + i);
                if (i > 3)
                    sortedThang.Add(100 + i);

                newList = original.SortBy(sortedThang, sortOption).ToList();

                if (i < 9)
                {
                    if (i > 7)
                    {
                        newList = newList.Skip(1).ToList();
                    }
                    else if (i > 5)
                    {
                        newList = newList.Take(newList.Count - 1).ToList();
                    }
                }

                if (i > 3)
                    sortedThang = sortedThang.Take(sortedThang.Count - 1).ToList();

                Assert.IsTrue(newList.SequenceEqual(sortedThang));
            }
        }
    }

    //=======================================================
    //Service provided by Telerik (www.telerik.com)
    //Conversion powered by NRefactory.
    //Twitter: @telerik
    //Facebook: facebook.com/telerik
    //=======================================================
}
