using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Threading;
using Microsoft.Diagnostics.Runtime;
using PixiEditor.Views;
using Exception = System.Exception;

namespace PixiEditor.Helpers;

public class DeadlockDetectionHelper
{
    private Dispatcher dispatcher;
    private Thread mainThread;
    private int checkTimes;
    private int errorsReported;
    
    public void Start()
    {
        dispatcher = MainWindow.Current.Dispatcher;
        mainThread = dispatcher.Thread;
        
        var thread = new Thread(ThreadStart)
        {
            Name = "Deadlock Detection Thread", IsBackground = true, Priority = ThreadPriority.BelowNormal
        };
        thread.Start();
    }

    private void ThreadStart()
    {
        while (true)
        {
            try
            {
                CheckStatus();
            }
            catch
            { }
            
            Thread.Sleep(200);
        }
    }

    private void CheckStatus()
    {
        if (CheckNotBusy())
        {
            return;
        }

        if (errorsReported < 5)
        {
            var task = Task.Run(() => ReportProblem(mainThread.ManagedThreadId));

            if (!task.Wait(TimeSpan.FromSeconds(8)))
            {
                StartDeadlockHandlerProcess();
            }
        }

        errorsReported++;

        CheckDispatcher(Timeout.Infinite, DispatcherPriority.Send);
    }

    private void StartDeadlockHandlerProcess()
    {
        Process process = new();

        process.StartInfo = new()
        {
            FileName = Path.ChangeExtension(Assembly.GetExecutingAssembly().Location, "exe"),
            Arguments = $"--deadlock {Process.GetCurrentProcess().Id} {mainThread.ManagedThreadId}"
        };

        process.Start();
    }
    
    private static void ReportProblem(int mainThreadId, int processId = -1)
    {
        string stackTrace;
        var isOwn = false;

        if (processId == -1)
        {
            isOwn = true;
            processId = Process.GetCurrentProcess().Id;
        }
        
        using (var target = DataTarget.CreateSnapshotAndAttach(processId))
        {
            stackTrace = GetMainClrThreadStackTrace(target, mainThreadId);
        }

        CrashHelper.SendExceptionInfoToWebhook(new DeadlockException(stackTrace, isOwn)).Wait();
    }

    private static string? GetMainClrThreadStackTrace(DataTarget target, int threadId)
    {
        foreach (var clr in target.ClrVersions)
        {
            var runtime = clr.CreateRuntime();
            foreach (var thread in runtime.Threads)
            {
                if (thread.ManagedThreadId != threadId)
                {
                    continue;
                }
                
                var builder = new StringBuilder();
                foreach (var frame in thread.EnumerateStackTrace().Take(100))
                {
                    builder.AppendLine(frame.ToString());
                }

                return builder.ToString();
            }
        }

        return null;
    }

    private bool CheckNotBusy()
    {
        var stopwatch = new Stopwatch();
        
        stopwatch.Start();
        bool isFree = CheckDispatcher(1000, DispatcherPriority.Background);
        stopwatch.Stop();

        if (isFree)
            return true;
        
        Debug.WriteLine($"-------- First deadlock check time [0] {stopwatch.Elapsed}");
        
        for (var i = 0; i < 3; i++)
        {
            stopwatch.Restart();
            isFree = CheckDispatcher(400, DispatcherPriority.Input);
            stopwatch.Stop();

            if (isFree)
                return true;
            
            Debug.WriteLine($"-------- Second deadlock check time [{i}] {stopwatch.Elapsed}");

        }
        
        
        isFree = CheckDispatcher(1600, DispatcherPriority.Input);

        stopwatch.Restart();
        Debug.WriteLine($"-------- Third deadlock check time [0] {stopwatch.Elapsed}");
        stopwatch.Stop();
        
        if (isFree)
            return true;

        isFree = CheckDispatcher(1000, DispatcherPriority.Send);

        stopwatch.Restart();
        Debug.WriteLine($"-------- Fourth deadlock check time [0] {stopwatch.Elapsed}");
        stopwatch.Stop();
        
        return isFree;
    }

    private bool CheckDispatcher(int timeout, DispatcherPriority priority)
    {
        var task = Task.Run(() => dispatcher.Invoke(ReturnTrue, TimeSpan.FromMilliseconds(timeout), priority) as bool? ?? false);

        var waitTimeout = (int)(timeout != -1 ? timeout * 1.5 : timeout);
        
        return task.Wait(waitTimeout) && task.Result;
    }

    private static bool ReturnTrue() => true;

    public static void HandleDeadlockOfOtherProcess(int processId, int mainThreadId) =>
        ReportProblem(mainThreadId, processId);
    
    class DeadlockException : Exception
    {
        public DeadlockException(string stackTrace, bool isOwn) : base(GetMessage(isOwn))
        {
            this.StackTrace = stackTrace;
        }

        public override string StackTrace { get; }

        private static string GetMessage(bool isOwn)
        {
            var builder = new StringBuilder("A deadlock has occured. Stack trace is from the Main Thread. ");
            if (!isOwn)
            {
                builder.Append(
                    "NOTICE: Any above state information is from the reporting process and not the actual deadlocked process");
            }

            return builder.ToString();
        }
    }
}
