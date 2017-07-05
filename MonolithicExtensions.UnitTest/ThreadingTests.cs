using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MonolithicExtensions.Windows;
using MonolithicExtensions.General;
using MonolithicExtensions.Portable;

namespace MonolithicExtensions.UnitTest
{
    [TestClass()]
    public class ThreadingTest : UnitTestBase
    {
        [TestMethod()]
        public void TestWaitOnAction()
        {
            //Dim startTime As DateTime
            DateTime failEnd = default(DateTime);
            Action testAction = () => { if (DateTime.Now < failEnd) throw new Exception("Still not time to complete"); };

            failEnd = DateTime.Now.AddMilliseconds(40);
            Assert.IsTrue(ThreadingServices.TryWaitOnAction(testAction, TimeSpan.FromSeconds(2)));

            failEnd = DateTime.Now.AddSeconds(10);
            Assert.IsFalse(ThreadingServices.TryWaitOnAction(testAction, TimeSpan.FromSeconds(1)));
        }

        [TestMethod()]
        public void TestForceSingleInstance()
        {
            int myValue = 10;
            AutoResetEvent firstTaskSignal = new AutoResetEvent(false);

            //The first task just waits around for 2 seconds, then sets a value. It SHOULD be able to lock
            Task firstTask = Task.Run(() => { ThreadingServices.LockGlobalMutexDuringAction(() => { firstTaskSignal.Set(); System.Threading.Thread.Sleep(2000); myValue = 20; }, TimeSpan.FromSeconds(1)); });

            firstTaskSignal.WaitOne();
            Assert.IsTrue(myValue == 10);

            //The second task immediately tries to set the value, HOWEVER since hte first task is still technically
            //running, the second task SHOULD throw an exception.
            MyAssert.ThrowsException(() => { ThreadingServices.LockGlobalMutexDuringAction(() => myValue = 30, TimeSpan.FromSeconds(1)); });

            Assert.IsTrue(myValue == 10);

            //Now just wait for the first to exit and ensure the value is updated accordingly
            firstTask.Wait();
            Assert.IsTrue(myValue == 20);

        }

        [TestMethod()]
        public void TestJobQueue()
        {
            const int TestCount = 100;
            AsyncJobQueue queue = new AsyncJobQueue();
            Random random = new Random();
            List<int> order = new List<int>();
            List<Task> tasks = new List<Task>();

            for (int i = 0; i <= TestCount - 1; i++)
            {
                int iLocal = i;
                int rLocal = random.Next();

                if ((random.Next() % 2) == 0)
                    System.Threading.Thread.Sleep(10);

                tasks.Add(queue.ExecuteWhenReady(() =>
                {
                    if ((rLocal % 5) == 0)
                        System.Threading.Thread.Sleep(rLocal % 100);
                    order.Add(iLocal);
                }));

                Logger.Info("Jobs running: " + queue.CurrentJobs.Count);

            }

            foreach (Task task in tasks)
            {
                task.Wait();
            }

            Logger.Info("Real order: " + string.Join(", ", order));

            for (var i = 1; i <= TestCount - 1; i++)
            {
                Assert.IsTrue(order[i] > order[i - 1]);
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
