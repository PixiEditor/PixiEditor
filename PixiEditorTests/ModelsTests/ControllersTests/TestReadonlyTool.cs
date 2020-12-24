using System;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools;

namespace PixiEditorTests.ModelsTests.ControllersTests
{
    public class TestReadonlyTool : ReadonlyTool
    {
        public TestReadonlyTool(Action toolAction)
        {
            ToolAction = toolAction;
        }

        public Action ToolAction { get; set; }

        public override void Use(Coordinates[] pixels)
        {
            ToolAction();
        }
    }
}