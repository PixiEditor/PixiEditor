using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Threading;
using Microsoft.Diagnostics.Runtime;
using PixiEditor.Extensions.Common.UserPreferences;
using PixiEditor.Models.DataHolders;
using PixiEditor.Views;
using Exception = System.Exception;
using ThreadState = System.Threading.ThreadState;

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
        DebugLogStopwatch stopwatch = null;
        DebugLogStopwatch.Create(ref stopwatch);
        
        // First deadlock check
        bool isFree = CheckDispatcher(1000, stopwatch, DispatcherPriority.Background);

        if (isFree)
            return true;
        
        stopwatch.Log("-------- First deadlock check time [0] {0}");
        
        // Second deadlock check
        for (var i = 0; i < 3; i++)
        {
            isFree = CheckDispatcher(400, stopwatch, DispatcherPriority.Input);

            if (isFree)
                return true;
            
            stopwatch.Log($"-------- Second deadlock check time [{i}] {{0}}");
        }
        
        // Third deadlock check
        isFree = CheckDispatcher(1600, stopwatch, DispatcherPriority.Input);

        if (isFree)
            return true;

        stopwatch.Log("-------- Third deadlock check time [0] {0}");
        
        // Forth deadlock
        int lastTimeout = mainThread.ThreadState.HasFlag(ThreadState.WaitSleepJoin) ? 1400 : 3500;
        isFree = CheckDispatcher(lastTimeout, stopwatch, DispatcherPriority.Send);

        stopwatch.Log("-------- Fourth deadlock check time [0] {0}");
        
        return isFree;
    }

    private bool CheckDispatcher(int timeout, DebugLogStopwatch stopwatch, DispatcherPriority priority)
    {
        stopwatch.Restart();
        var task = Task.Run(() => dispatcher.Invoke(ReturnTrue, TimeSpan.FromMilliseconds(timeout), priority) as bool? ?? false);

        var waitTimeout = (int)(timeout != -1 ? timeout * 1.2 : timeout);
        
        bool result = task.Wait(waitTimeout) && task.Result;
        stopwatch.Stop();

        return result;
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
