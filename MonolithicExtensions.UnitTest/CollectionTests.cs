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

    /*using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    namespace PortableExtensions.UnitTest
    {
        [TestClass] public Class CollectionsTest

        [TestMethod] public void TestIsEquivalent_List()

            Dim myList As New List(Of Integer)(SimpleList)
            Dim myList2 As New List(Of Integer)(SimpleList)

            myList2.Sort()

            Assert.IsFalse(myList.SequenceEqual(myList2))
            Assert.IsTrue(myList.IsEquivalentTo(myList2))

            myList.Add(7)
            Assert.IsFalse(myList.IsEquivalentTo(myList2))

            myList2.Add(2)
            Assert.IsFalse(myList.IsEquivalentTo(myList2))

        End void

        [TestMethod] public void TestIsEquivalent_Dictionary()

            Dim myDict As New Dictionary(Of Integer, String)(SimpleDictionary)
            Dim myDict2 As New SortedDictionary(Of Integer, String)(SimpleDictionary)

            Assert.IsFalse(myDict.ToList().SequenceEqual(myDict2.ToList()))
            Assert.IsTrue(myDict.IsEquivalentTo(myDict2))

            myDict.Add(8, "gr8 b8 m8 r8 8/8")
            Assert.IsFalse(myDict.IsEquivalentTo(myDict2))

            myDict2.Add(8, "gr9 b9 m9 r9 9/9")
            Assert.IsFalse(myDict.IsEquivalentTo(myDict2))

        End Sub

        [TestMethod] public void TestShuffle()

            'Create an array of a hundred integers. Create a copy of it too.
            Dim original As New List(Of Integer)
            For i = 0 To 100 : original.Add(i) : Next
            Dim tester As New List(Of Integer)(original)
            Dim testerPrevious As New List(Of Integer)(original)

            'Make sure they're the same before continuing
            Assert.IsTrue(original.SequenceEqual(tester))

            'Perform a few shuffles and make sure they're always different but that the collection is always the same
            For i = 0 To 100
                tester.Shuffle()
                Assert.IsFalse(tester.SequenceEqual(original))
                Assert.IsFalse(tester.SequenceEqual(testerPrevious))
                Assert.IsTrue(tester.IsEquivalentTo(original))
                Assert.IsTrue(tester.IsEquivalentTo(testerPrevious))
                testerPrevious = New List(Of Integer)(tester)
            Next

        End Sub

        [TestMethod] public void TestSortBy()

            Dim original As List(Of Integer)
            Dim sortedThang As List(Of Integer)
            Dim sortOption As SortByExcessOption
            Dim newList As List(Of Integer)

            For i = 0 To 10

                original = New List(Of Integer)(SimpleList)
                sortedThang = New List(Of Integer)(SimpleList)
                sortOption = SortByExcessOption.PlaceAtEnd
                sortedThang.Shuffle()

                If i >= 9 Then
                    sortOption = SortByExcessOption.LeaveOut
                ElseIf i > 7 Then
                    sortOption = SortByExcessOption.PlaceAtBeginning
                End If

                If i > 5 Then original.Add(200 + i)
                If i > 3 Then sortedThang.Add(100 + i)

                newList = original.SortBy(sortedThang, sortOption).ToList()

                If i< 9 Then
                    If i > 7 Then
                        newList = newList.Skip(1).ToList()
                    ElseIf i > 5 Then
                        newList = newList.Take(newList.Count - 1).ToList()
                    End If
                End If

                If i > 3 Then sortedThang = sortedThang.Take(sortedThang.Count - 1).ToList()

                Assert.IsTrue(newList.SequenceEqual(sortedThang))

            Next

        End Sub

    End Class

    }*/
}
