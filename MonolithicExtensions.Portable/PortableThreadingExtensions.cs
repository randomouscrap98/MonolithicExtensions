using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MonolithicExtensions.Portable
{
    /// <summary>
    /// Allows jobs to be run one at a time, no matter when the job was submitted. Waits for
    /// jobs to be scheduled using async callbacks.
    /// </summary>
    public class AsyncJobQueue
    {
        private readonly object QueueLock = new object();

        private Queue<Tuple<Action, AutoResetEvent>> JobQueue = new Queue<Tuple<Action, AutoResetEvent>>();

        public async Task ExecuteWhenReady(Action job)
        {
            AutoResetEvent myEvent = new AutoResetEvent(false);
            AutoResetEvent currentJobEvent = default(AutoResetEvent);

            //Grab the next signaler and add us as the last job
            lock (QueueLock)
            {
                if (JobQueue.Count == 0)
                {
                    currentJobEvent = new AutoResetEvent(true);
                }
                else
                {
                    currentJobEvent = JobQueue.Last().Item2;
                }
                JobQueue.Enqueue(Tuple.Create(job, myEvent));
            }

            //Wait for our turn
            await Task.Run(() => currentJobEvent.WaitOne());

            try
            {
                //Run our crap
                job.Invoke();
            }
            finally
            {
                //Remove ourselves from the queue FIRST, that way if we're the only thing in the queue and a new 
                //job gets added, it will either attach to our event which HASN'T been set yet, or it'll see 
                //nothing in the queue and not have any wait.
                lock (QueueLock)
                {
                    dynamic dequeuedJob = JobQueue.Dequeue();
                    if (!object.ReferenceEquals(dequeuedJob.Item1, job))
                    {
                        throw new InvalidOperationException("The queue had a programming failure; the dequeued job is not the same as the one that just completed!");
                    }
                }

                //Tell the next person in line to go.
                myEvent.Set();
            }
        }

        /// <summary>
        /// Completely removes all jobs from the job queue, including the one running (it'll still finish though)
        /// </summary>
        public void ClearJobs()
        {
            lock (QueueLock)
            {
                JobQueue.Clear();
            }
        }

        public List<Action> CurrentJobs
        {
            get
            {
                lock (QueueLock)
                {
                    return JobQueue.Select(x => x.Item1).ToList();
                }
            }
        }

    }
}
