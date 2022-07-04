using Microsoft.Extensions.DependencyInjection;
using PixiEditor.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PixiEditorTests.HelpersTests;

[Collection("Application collection")]
public class DependencyInjectionTests
{
    private class TestService
    {
    }

    private class TestInjectable
    {
        public TestService TestService { get; }

        public TestInjectable(TestService service)
        {
            TestService = service;
        }
    }

    [Fact]
    public void TestThatInjectingWorks()
    {
        IServiceProvider provider = new ServiceCollection()
            .AddSingleton<TestService>()
            .BuildServiceProvider();

        TestInjectable injectable = provider.Inject<TestInjectable>();

        Assert.NotNull(injectable.TestService);
    }
}