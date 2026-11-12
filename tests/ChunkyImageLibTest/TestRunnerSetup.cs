using Xunit;
using Xunit.Sdk;
using Xunit.v3;

[assembly: CollectionBehavior(CollectionBehavior.CollectionPerAssembly, DisableTestParallelization = true, MaxParallelThreads = 1)]

namespace ChunkyImageLibTest;

public class TestRunnerSetup
{
}