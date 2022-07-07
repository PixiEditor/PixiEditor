using System.Windows.Input;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Toolbars;

namespace PixiEditor.ViewModels.SubViewModels.Tools.Tools;

[Command.Tool(Key = Key.E)]
internal class EraserToolViewModel : ToolViewModel
{
    public EraserToolViewModel()
    {
        ActionDisplay = "Draw to remove color from a pixel.";
        Toolbar = new BasicToolbar();
    }

    public override string Tooltip => $"Erasers color from pixel. ({Shortcut})";
}
