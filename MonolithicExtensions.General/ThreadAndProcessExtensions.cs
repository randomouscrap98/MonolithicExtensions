using MonolithicExtensions.Portable.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MonolithicExtensions.General
{
    public static class ThreadingServices
    {
        /// <summary>
        /// Given an Action which can throw exceptions, this function executes that Action repeatedly until either
        /// it succeeds or the timeout is reached. The interval for execution is 1 millisecond (or whatever granularity
        /// the OS provides for the shortest sleep time). This is a blocking function.
        /// </summary>
        /// <param name="PerformingAction"></param>
        /// <param name="WaitTime"></param>
        /// <returns>Whether or not the Action completed successfully</returns>
        public static bool TryWaitOnAction(Action PerformingAction, TimeSpan WaitTime)
        {
            try
            {
                WaitOnAction(PerformingAction, WaitTime);
                return true;
            }
            catch //(Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Given an Action which can throw exceptions, this function executes that Action repeatedly until either
        /// it succeeds or the timeout is reached. The interval for execution is 1 millisecond (or whatever granularity
        /// the OS provides for the shortest sleep time). This is a blocking function.
        /// </summary>
        /// <param name="PerformingAction"></param>
        /// <param name="WaitTime"></param>
        public static void WaitOnAction(Action PerformingAction, TimeSpan WaitTime)
        {
            Exception lastException = new Exception();
            DateTime beginTime = DateTime.Now;
            while ((DateTime.Now - beginTime) < WaitTime)
            {
                try
                {
                    PerformingAction.Invoke();
                    return;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    System.Threading.Thread.Sleep(1);
                }
            }
            throw lastException;
        }

        /// <summary>
        /// Ensures that the given code will only be run by one thread/process/instance of the current application. This is useful
        /// for forcing a program to only run one copy of itself.
        /// </summary>
        /// <param name="runAction"></param>
        /// <param name="startTimeout"></param>
        /// <remarks>To run only a single instance of an application, simply pass in the entirety of the running code as the 
        /// first action. The entirety of this function's code was converted from http://stackoverflow.com/a/229567/1066474 </remarks>
        public static void LockGlobalMutexDuringAction(Action runAction, TimeSpan startTimeout)
        {
            //get application GUID as defined in AssemblyInfo.cs
            string appGuid = ((GuidAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(GuidAttribute), false).GetValue(0)).Value.ToString();

            // unique id for global mutex - Global prefix means it Is global to the machine
            string mutexId = string.Format(@"Global\{{{0}}}", appGuid);

            // Need a place to store a return value in Mutex() constructor call
            bool createdNew = false;

            //This allows all users to run this process, but only once per user (?)
            MutexAccessRule allowEveryoneRule = new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), MutexRights.FullControl, AccessControlType.Allow);
            MutexSecurity securitySettings = new MutexSecurity();
            securitySettings.AddAccessRule(allowEveryoneRule);

            using (Mutex mutex = new Mutex(false, mutexId, out createdNew, securitySettings))
            {
                bool hasHandle = false;

                try
                {
                    try
                    {
                        hasHandle = mutex.WaitOne(Convert.ToInt32(startTimeout.TotalMilliseconds), false);
                        if (hasHandle == false)
                        {
                            throw new TimeoutException("Timeout waiting for exclusive access");
                        }
                    }
                    catch (AbandonedMutexException)
                    {
                        // Log the fact that the mutex was abandoned in another process, it will still get acquired
                        hasHandle = true;
                    }

                    // Perform your work here.
                    runAction.Invoke();
                }
                finally
                {
                    if (hasHandle)
                        mutex.ReleaseMutex();
                }
            }
        }

        public static void Timeout(TimeSpan time, Action<CancellationToken> runner, CancellationToken token)
        {
            var timer = new System.Timers.Timer(time.TotalMilliseconds);
            timer.Elapsed += (o, e) =>
            {
                timer.Enabled = false;

                if(!token.IsCancellationRequested)
                    runner.Invoke(token);

                timer.Dispose();
            };
            timer.Enabled = true;
        }

        /// <summary>
        /// WARNING: Non-cancellable. Try not to use this version unless you're really lazy and really confident.
        /// </summary>
        /// <param name="time"></param>
        /// <param name="runner"></param>
        public static void Timeout(TimeSpan time, Action runner)
        {
            var timer = new System.Timers.Timer(time.TotalMilliseconds);
            timer.Elapsed += (o, e) =>
            {
                timer.Enabled = false;
                runner.Invoke();
                timer.Dispose();
            };
            timer.Enabled = true;
        }
    }

    public static class ProcessServices
    {
        private static ILogger Logger = LogServices.CreateLoggerFromDefault(typeof(ProcessServices));

        public static TimeSpan ProcessPollingInterval = TimeSpan.FromSeconds(1);

        public static bool MightBeService(this Process TestProcess)
        {
            TestProcess.Refresh();
            return TestProcess.MainWindowHandle == IntPtr.Zero;
        }

        public class ProcessResult
        {
            public int ExitCode = 0;
            public string Output = null;
            public string Error = null;
        }
        
        public static async Task<ProcessResult> StartProcess(string executable, string arguments, CancellationToken token, string workingDirectory = null)//, bool separateExecution)
        {
            Logger.Trace($"StartProcess called for executable {executable} and arguments {arguments}");
            var info = new ProcessStartInfo(executable, arguments);

            //if (separateExecution)
            //{
            //    Logger.Debug($"Executable {executable} will be started independently and will not be monitored");
            //    testProcessInfo.UseShellExecute = True
            //    testProcessInfo.RedirectStandardError = False
            //    testProcessInfo.RedirectStandardOutput = False
            //    testProcessInfo.CreateNoWindow = False
            //}
            //Else
            info.UseShellExecute = false;
            info.RedirectStandardError = true;
            info.RedirectStandardOutput = true;
            info.CreateNoWindow = true;
            //End If

            var executableDirectory = System.IO.Path.GetDirectoryName(executable);

            if (!string.IsNullOrWhiteSpace(workingDirectory))
                info.WorkingDirectory = workingDirectory;
            else if (!string.IsNullOrWhiteSpace(executableDirectory))
                info.WorkingDirectory = executableDirectory;

            var process = new Process();
            process.StartInfo = info;

            var errors = new StringBuilder();
            var output = new StringBuilder();
            var result = new ProcessResult();

            using (var outputWaitHandle = new AutoResetEvent(false))
            {
                using (var errorWaitHandle = new AutoResetEvent(false))
                {
                    //What to do when the process buffers a line of stdout
                    var outputHandler = new DataReceivedEventHandler((s, e) => //Action<object, DataReceivedEventArgs>((s, e) =>
                    {
                        if (e.Data == null)
                            outputWaitHandle.Set();
                        else
                            output.AppendLine(e.Data);
                    });

                    //What to do when the process buffers a line of stderr
                    var errorHandler = new DataReceivedEventHandler((s, e) => //new Action<object, DataReceivedEventArgs>((s, e) =>
                    {
                        if (e.Data == null)
                            errorWaitHandle.Set();
                        else
                            errors.AppendLine(e.Data);
                    });

                    bool processExited = false;
                    bool outputExited = false;

                    try
                    {
                        process.OutputDataReceived += outputHandler;
                        process.ErrorDataReceived += errorHandler;

                        //NOW start the process since... you know, we have the handlers
                        process.Start();
                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();

                        //Nice kinda asynchronous whatever. IDK
                        await Task.Run(() =>
                        {
                            //Keep polling the process until it exits. Or quit entirely if the user cancelled.
                            while (!processExited)
                            {
                                processExited = process.WaitForExit((int)ProcessPollingInterval.TotalMilliseconds); //WaitForExit(CType(ExecutableTimeout.TotalMilliseconds, Integer))
                                if(token.IsCancellationRequested && !processExited)
                                {
                                    Logger.Warn($"User cancelled process {executable} before process was complete!");
                                    break;
                                }
                            }

                            outputExited = outputWaitHandle.WaitOne(ProcessPollingInterval) && errorWaitHandle.WaitOne(ProcessPollingInterval);
                        });

                        //Force close the process if it didn't exit nicely.
                        if(!processExited)
                        {
                            process.Close();
                            throw new InvalidOperationException("Cancelled before process was able to finish!");
                        }
                    }
                    finally
                    {
                        process.OutputDataReceived -= outputHandler;
                        process.ErrorDataReceived -= errorHandler;
                    }

                    result.Output = output.ToString();
                    result.Error = errors.ToString();
                    result.ExitCode = process.ExitCode;

                    Logger.Debug($"Output for executable {executable} (exit code {result.ExitCode}): {result.Output}");

                    if (!string.IsNullOrEmpty(result.Error))
                        Logger.Warn($"Error output for executable {executable}: {result.Error}");

                    return result;
                }
            }
        }
    }

    ///// <summary>
    ///// Allows jobs to be run one at a time, no matter when the job was submitted. Waits for
    ///// jobs to be scheduled using async callbacks.
    ///// </summary>
    //public class AsyncJobQueue
    //{

    //    private readonly object QueueLock = new object();

    //    private Queue<Tuple<Action, AutoResetEvent>> JobQueue = new Queue<Tuple<Action, AutoResetEvent>>();
    //    //Private NextSignaler As New AutoResetEvent(True)

    //    //Private Async Function WaitForToken() As Task(Of AutoResetEvent)

    //    //    Dim waitSignaler As AutoResetEvent

    //    //    'Retrieve the signaler WE should be using to wait on, then swap out the next signaler with
    //    //    'our own (so the next job waits on us)
    //    //    SyncLock SignalLock
    //    //        mySignaler = New AutoResetEvent(False)
    //    //        waitSignaler = NextSignaler
    //    //        NextSignaler = mySignaler
    //    //    End SyncLock

    //    //    'Now we can safely wait for our signal.
    //    //    Return mySignaler

    //    //End Function

    //    public async Task ExecuteWhenReady(Action job)
    //    {
    //        AutoResetEvent myEvent = new AutoResetEvent(false);
    //        AutoResetEvent currentJobEvent = default(AutoResetEvent);

    //        //Grab the next signaler and add us as the last job
    //        lock (QueueLock)
    //        {
    //            if (JobQueue.Count == 0)
    //            {
    //                currentJobEvent = new AutoResetEvent(true);
    //            }
    //            else
    //            {
    //                currentJobEvent = JobQueue.Last().Item2;
    //            }
    //            JobQueue.Enqueue(Tuple.Create(job, myEvent));
    //        }

    //        //Wait for our turn
    //        await Task.Run(() => currentJobEvent.WaitOne());

    //        try
    //        {
    //            //Run our crap
    //            job.Invoke();
    //        }
    //        finally
    //        {
    //            //Remove ourselves from the queue FIRST, that way if we're the only thing in the queue and a new 
    //            //job gets added, it will either attach to our event which HASN'T been set yet, or it'll see 
    //            //nothing in the queue and not have any wait.
    //            lock (QueueLock)
    //            {
    //                dynamic dequeuedJob = JobQueue.Dequeue();
    //                if (!object.ReferenceEquals(dequeuedJob.Item1, job))
    //                {
    //                    throw new InvalidOperationException("The queue had a programming failure; the dequeued job is not the same as the one that just completed!");
    //                }
    //            }

    //            //Tell the next person in line to go.
    //            myEvent.Set();
    //        }
    //    }

    //    /// <summary>
    //    /// Completely removes all jobs from the job queue, including the one running (it'll still finish though)
    //    /// </summary>
    //    public void ClearJobs()
    //    {
    //        lock (QueueLock)
    //        {
    //            JobQueue.Clear();
    //        }
    //    }

    //    public List<Action> CurrentJobs
    //    {
    //        get
    //        {
    //            lock (QueueLock)
    //            {
    //                return JobQueue.Select(x => x.Item1).ToList();
    //            }
    //        }
    //    }

    //}

    //=======================================================
    //Service provided by Telerik (www.telerik.com)
    //Conversion powered by NRefactory.
    //Twitter: @telerik
    //Facebook: facebook.com/telerik
    //=======================================================
}
