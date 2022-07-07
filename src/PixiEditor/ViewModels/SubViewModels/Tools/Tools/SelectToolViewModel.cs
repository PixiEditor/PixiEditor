using System.Windows.Input;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Toolbars;

namespace PixiEditor.ViewModels.SubViewModels.Tools.Tools;

[Command.Tool(Key = Key.M)]
internal class SelectToolViewModel : ToolViewModel
{
    public SelectToolViewModel()
    {
        ActionDisplay = "Click and move to select an area.";
        Toolbar = new SelectToolToolbar();
    }

    public SelectionMode SelectionType { get; set; } = SelectionMode.Add;

    public override string Tooltip => $"Selects area. ({Shortcut})";
}
