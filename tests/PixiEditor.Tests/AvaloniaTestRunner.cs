using System.Reflection;
using Avalonia.Headless;
using Avalonia.Platform;
using Avalonia.Threading;
using PixiEditor.Desktop;
using Xunit.Abstractions;
using Xunit.Sdk;

[assembly:TestFramework("PixiEditor.Tests.AvaloniaTestRunner", "PixiEditor.Tests")]
[assembly:CollectionBehavior(CollectionBehavior.CollectionPerAssembly, DisableTestParallelization = false, MaxParallelThreads = 1)]
namespace PixiEditor.Tests
{
    public class AvaloniaTestRunner : XunitTestFramework
    {
        public AvaloniaTestRunner(IMessageSink messageSink) : base(messageSink)
        {
        }

        protected override ITestFrameworkExecutor CreateExecutor(AssemblyName assemblyName)
            => new Executor(assemblyName, SourceInformationProvider, DiagnosticMessageSink);


        class Executor : XunitTestFrameworkExecutor
        {
            public Executor(AssemblyName assemblyName, ISourceInformationProvider sourceInformationProvider,
                IMessageSink diagnosticMessageSink) : base(assemblyName, sourceInformationProvider,
                diagnosticMessageSink)
            {
            }

            protected override async void RunTestCases(IEnumerable<IXunitTestCase> testCases,
                IMessageSink executionMessageSink,
                ITestFrameworkExecutionOptions executionOptions)
            {
                executionOptions.SetValue("xunit.execution.DisableParallelization", false);
                using (var assemblyRunner = new Runner(
                    TestAssembly, testCases, DiagnosticMessageSink, executionMessageSink,
                    executionOptions)) await assemblyRunner.RunAsync();
            }
        }

        class Runner : XunitTestAssemblyRunner
        {
            public Runner(ITestAssembly testAssembly, IEnumerable<IXunitTestCase> testCases,
                IMessageSink diagnosticMessageSink, IMessageSink executionMessageSink,
                ITestFrameworkExecutionOptions executionOptions) : base(testAssembly, testCases, diagnosticMessageSink,
                executionMessageSink, executionOptions)
            {
            }


            protected override void SetupSyncContext(int maxParallelThreads)
            {
                var tcs = new TaskCompletionSource<SynchronizationContext>();
                new Thread(() =>
                {
                    try
                    {
                        Program.BuildAvaloniaApp()
                            .UseHeadless(new AvaloniaHeadlessPlatformOptions { FrameBufferFormat = PixelFormat.Bgra8888, UseHeadlessDrawing = false })
                            .SetupWithoutStarting();
                        tcs.SetResult(SynchronizationContext.Current);
                    }
                    catch (Exception e)
                    {
                        tcs.SetException(e);
                    }
                    Dispatcher.UIThread.MainLoop(CancellationToken.None);
                })
                {
                    IsBackground = true
                }.Start();

                SynchronizationContext.SetSynchronizationContext(tcs.Task.Result);
            }


        }
    }
}
