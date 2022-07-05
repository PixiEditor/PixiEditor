using System;
using ChunkyImageLib.DataHolders;
using PixiEditor.Models.Tools;

namespace PixiEditorTests.ModelsTests.ControllersTests;

public class TestReadonlyTool : ReadonlyTool
{
    public override string Tooltip => "";

    public TestReadonlyTool(Action toolAction)
    {
        ToolAction = toolAction;
    }

    public Action ToolAction { get; set; }

    public override void Use(VecD pos)
    {
        ToolAction();
    }
}
