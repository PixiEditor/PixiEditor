using Xunit;

[assembly:
    CollectionBehavior(CollectionBehavior.CollectionPerAssembly, DisableTestParallelization = false,
        MaxParallelThreads = 1)]

namespace ChunkyImageLibTest;

public class TestRunnerSetup
{
}