using System;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools;
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

    public class TestReadonlyTool : ReadonlyTool
    {
        public TestReadonlyTool(Action toolAction)
        {
            ToolAction = toolAction;
        }

        public Action ToolAction { get; set; }

        public override ToolType ToolType => ToolType.Select;

        public override void Use(Coordinates[] pixels)
        {
            ToolAction();
        }
    }
}