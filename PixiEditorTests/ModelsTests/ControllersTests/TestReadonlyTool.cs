using System;
using System.Collections.Generic;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools;

namespace PixiEditorTests.ModelsTests.ControllersTests
{
    public class TestReadonlyTool : ReadonlyTool
    {
        public override string Tooltip => "";

        public TestReadonlyTool(Action toolAction)
        {
            ToolAction = toolAction;
        }

        public Action ToolAction { get; set; }

        public override void Use(IReadOnlyList<Coordinates> pixels)
        {
            ToolAction();
        }
    }
}
