using Moq;
using PixiEditor.UpdateModule;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PixiEditorTests.UpdateModuleTests
{
    public class UpdateCheckerTests
    {
        [Theory]
        [InlineData("0.1.2", "0.1.2", false)]
        [InlineData("0.5.", "0.1.2", true)]
        [InlineData(null, "0.1.2", true)]
        public void TestThatCheckUpdateAvailableChecksCorrectly(string newVersion, string currentVersion, bool expectedValue)
        {
            UpdateChecker checker = new UpdateChecker(currentVersion);
            bool result = checker.CheckUpdateAvailable(new ReleaseInfo() { TagName = newVersion });
            Assert.True(result == expectedValue);
        }
    }
}
