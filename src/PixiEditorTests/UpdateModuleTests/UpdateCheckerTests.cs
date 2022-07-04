using Moq;
using PixiEditor.UpdateModule;
using System.Threading.Tasks;
using Xunit;

namespace PixiEditorTests.UpdateModuleTests
{
    public class UpdateCheckerTests
    {
        //[Theory]
        //[InlineData("0.1.2", "0.1.2", false)]
        //[InlineData("0.5", "0.1.2", false)]
        //[InlineData("0.1.3", "0.1.2", false)]
        //[InlineData("0.1.2", "0.1.3", true)]
        //[InlineData("0.2.1", "0.1.3", false)]
        //public void TestThatCheckUpdateAvailableChecksCorrectly(string currentVersion, string newVersion, bool expectedValue)
        //{
        //    UpdateChecker checker = new UpdateChecker(currentVersion);
        //    bool result = checker.CheckUpdateAvailable(new ReleaseInfo(true) { TagName = newVersion });
        //    Assert.True(result == expectedValue);
        //}

        //[Theory]
        //[InlineData("0.1.2", "0.1.2", false)]
        //[InlineData("0.5", "0.1.2", false)]
        //[InlineData("0.1.3", "0.1.2", false)]
        //[InlineData("0.1.2", "0.1.3", true)]
        //[InlineData("0.2.1", "0.1.3", false)]
        //public void CheckThatVersionBiggerComparesCorrectly(string currentVersion, string newVersion, bool expectedValue)
        //{
        //    Assert.True(UpdateChecker.VersionBigger(currentVersion, newVersion) == expectedValue);
        //}

        //[Theory]
        //[InlineData("0.1.3.5", new string[] { "" }, true)]
        //[InlineData("0.1.3.5", new string[] { "0.1.3.5" }, false)]
        //[InlineData("0.1.0.0", new string[] { "0.1.3.5", "0.4.2.1" }, true)]
        //[InlineData("0.1.2.2", new string[] { " 0.1.2.2 " }, false)]
        //public void TestThatIsUpdateCompatibleChecksVersionCorrectly(string version, string[] incompatibleVersions, bool expectedResult)
        //{
        //    UpdateChecker checker = new UpdateChecker(version);
        //    Assert.Equal(expectedResult, checker.IsUpdateCompatible(incompatibleVersions));
        //}
    }
}