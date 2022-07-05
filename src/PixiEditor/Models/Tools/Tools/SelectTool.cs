using System.Windows.Input;
using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.Models.Commands.Attributes;
using PixiEditor.Models.Tools.ToolSettings.Toolbars;
using SkiaSharp;

namespace PixiEditor.Models.Tools.Tools;

[Command.Tool(Key = Key.M)]
internal class SelectTool : ReadonlyTool
{
    public SelectTool()
    {
        ActionDisplay = "Click and move to select an area.";
        Toolbar = new SelectToolToolbar();
    }

    public SelectionMode SelectionType { get; set; } = SelectionMode.Add;

    public override string Tooltip => $"Selects area. ({Shortcut})";

    public override void BeforeUse()
    {
    }

    public override void AfterUse(SKRectI sessionRect)
    {
    }

    public override void Use(VecD pos)
    {
    }
}
