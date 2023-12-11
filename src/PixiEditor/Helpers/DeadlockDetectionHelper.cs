using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Diagnostics.Runtime;
using PixiEditor.Extensions.Common.UserPreferences;
using PixiEditor.Models.DataHolders;
using PixiEditor.Views;
using Exception = System.Exception;
using ThreadState = System.Threading.ThreadState;

namespace PixiEditor.Helpers;

internal class DeadlockDetectionHelper
{
    private Dispatcher dispatcher;
    private Thread mainThread;
    private int checkTimes;
    private int errorsReported;
    private bool shuttingDown;

    private int totalChecks;
    private int secondStageChecks;
    private int thirdStageChecks;
    private int fourthStageChecks;
    private int deadlocksDetected;
    
    public DateTime StartTime { get; private set; }
    
    public static DeadlockDetectionHelper Current { get; private set; }

    public int TotalChecks => totalChecks;
    
    public int SecondStageChecks => secondStageChecks;

    public int ThirdStageChecks => thirdStageChecks;

    public int FourthStageChecks => fourthStageChecks;

    public int DeadlocksDetected => deadlocksDetected;
    
    public DeadlockDetectionHelper()
    {
        if (Current != null)
        {
            throw new InvalidOperationException("There's already a deadlock detection helper");
        }
        
        Current = this;
    }
    
    public void Start()
    {
        var application = Application.Current;

        application.Exit += (_, _) => shuttingDown = true;
        dispatcher = application.Dispatcher;
        mainThread = dispatcher.Thread;
        
        var thread = new Thread(ThreadStart)
        {
            Name = "Deadlock Detection Thread", IsBackground = true, Priority = ThreadPriority.BelowNormal
        };
        thread.Start();
    }

    private void ThreadStart()
    {
        StartTime = DateTime.Now;
        
        while (true)
        {
            if (shuttingDown)
            {
                return;
            }
            
            try
            {
                CheckStatus();
            }
            catch
            {
                if (Debugger.IsAttached)
                    Debugger.Break();
            }
            
            Thread.Sleep(200);
        }
    }

    private void CheckStatus()
    {
        if (CheckNotBusy())
        {
            return;
        }

        Interlocked.Increment(ref deadlocksDetected);

        if (errorsReported < 5)
        {
            TryReportProblem();
        }

        errorsReported++;

        TrySaveFilesForNextSession();

        if (Debugger.IsAttached)
        {
            CheckDispatcher(Timeout.Infinite, null, DispatcherPriority.Send);
        }
        else
        {
            int timeout = mainThread.ThreadState.HasFlag(ThreadState.WaitSleepJoin) ? 8500 : 25000;
            bool close = !CheckDispatcher(timeout, null, DispatcherPriority.Send);
        
            if (close)
            {
                ForceNewProcess();
                Environment.FailFast("Encountered deadlock. Reopening in new process");
            }
        }
    }

    private void TryReportProblem()
    {
        var thread = new Thread(() =>
        {
            ReportProblem(mainThread.ManagedThreadId);
        });
        
        thread.Start();
        thread.Join(7000);
    }

    private void TrySaveFilesForNextSession()
    {
        var thread = new Thread(() =>
        {
            var viewModel = ViewModelMain.Current;
            
            var list = new List<AutosaveFilePathInfo>();
            foreach (var document in viewModel.DocumentManagerSubViewModel.Documents)
            {
                document.AutosaveViewModel.PanicAutosave();
                if (document.AutosaveViewModel.LastSavedPath != null || document.FullFilePath != null)
                {
                    list.Add(new AutosaveFilePathInfo(document.FullFilePath, document.AutosaveViewModel.LastSavedPath));
                }
            }
        
            IPreferences.Current?.UpdateLocalPreference(PreferencesConstants.UnsavedNextSessionFiles, list);
        });
        
        thread.Start();
        thread.Join(10000);
    }

    private void ForceNewProcess()
    {
        Process process = new();

        process.StartInfo = new()
        {
            FileName = Path.ChangeExtension(Assembly.GetExecutingAssembly().Location, "exe"),
            Arguments = "--force-new-instance --wait-before-init 6000"
        };

        process.Start();
    }
    
    private static void ReportProblem(int mainThreadId)
    {
        string stackTrace;

        using (var target = DataTarget.CreateSnapshotAndAttach(Process.GetCurrentProcess().Id))
        {
            stackTrace = GetMainClrThreadStackTrace(target, mainThreadId);
        }

        CrashHelper.SendExceptionInfoToWebhook(new DeadlockException(stackTrace)).Wait();
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
        DebugLogStopwatch stopwatch = null;
        DebugLogStopwatch.Create(ref stopwatch);
        
        // First deadlock check
        bool isFree = CheckDispatcher(1000, stopwatch, DispatcherPriority.Background);

        Interlocked.Increment(ref totalChecks);

        if (isFree)
            return true;
            
        stopwatch.Log($"----- First deadlock check time [0] {{0}}; Dispatcher had time: false");
        
        Interlocked.Increment(ref secondStageChecks);
        
        // Second deadlock check
        for (var i = 0; i < 3; i++)
        {
            isFree = CheckDispatcher(400, stopwatch, DispatcherPriority.Input);

            stopwatch.Log($"------ Second deadlock check time [{i}] {{0}}; Dispatcher had time: {isFree}");
            
            if (isFree)
                return true;
        }
        
        Interlocked.Increment(ref thirdStageChecks);

        // Third deadlock check
        isFree = CheckDispatcher(1600, stopwatch, DispatcherPriority.Input);

        stopwatch.Log($"------- Third deadlock check time [0] {{0}}; Dispatcher had time: {isFree}");

        if (isFree)
            return true;

        Interlocked.Increment(ref fourthStageChecks);

        // Forth deadlock
        int lastTimeout = mainThread.ThreadState.HasFlag(ThreadState.WaitSleepJoin) ? 1400 : 3500;
        isFree = CheckDispatcher(lastTimeout, stopwatch, DispatcherPriority.Send);

        stopwatch.Log($"-------- Fourth deadlock check time [0] {{0}}; Dispatcher had time: {isFree}");
        
        return isFree;
    }

    private bool CheckDispatcher(int timeout, DebugLogStopwatch stopwatch, DispatcherPriority priority)
    {
        if (dispatcher.HasShutdownStarted)
        {
            Debug.WriteLine("----- Deadlock detector could not check for deadlock as the main dispatcher is shutting down");
            return true;
        }
        
        stopwatch.Restart();
        var task = Task.Run(() => dispatcher.Invoke(ReturnTrue, TimeSpan.FromMilliseconds(timeout), priority) as bool? ?? false);

        var waitTimeout = (int)(timeout != -1 ? timeout * 1.2 : timeout);
        
        bool result = task.Wait(waitTimeout) && task.Result;
        stopwatch.Stop();

        return result;
    }

    private static bool ReturnTrue() => true;

    class DeadlockException : Exception
    {
        public DeadlockException(string stackTrace) : base("A deadlock has occured. Stack trace is from the Main Thread. ")
        {
            this.StackTrace = stackTrace;
        }

        public override string StackTrace { get; }
    }
}
