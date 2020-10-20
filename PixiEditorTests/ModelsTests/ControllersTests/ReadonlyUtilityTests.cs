using PixiEditor.Models.Controllers;
using PixiEditor.Models.Position;
using Xunit;

namespace PixiEditorTests.ModelsTests.ControllersTests
{
    public class ReadonlyUtilityTests
    {
        [Fact]
        public void TestThatExecuteToolExecutesTool()
        {
            bool toolUsed = false;

            ReadonlyToolUtility util = new ReadonlyToolUtility();
            util.ExecuteTool(new[] { new Coordinates(0, 0) }, new TestReadonlyTool(() => toolUsed = true));
            Assert.True(toolUsed);
        }
    }
}