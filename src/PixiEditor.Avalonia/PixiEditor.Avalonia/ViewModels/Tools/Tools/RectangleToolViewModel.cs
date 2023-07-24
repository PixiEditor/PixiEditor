using System.Windows.Input;
using Avalonia.Input;
using ChunkyImageLib.DataHolders;
using PixiEditor.Avalonia.ViewModels;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Containers.Toolbars;
using PixiEditor.Models.Containers.Tools;
using PixiEditor.Models.Localization;

namespace PixiEditor.ViewModels.SubViewModels.Tools.Tools;

[Command.Tool(Key = Key.R)]
internal class RectangleToolViewModel : ShapeTool, IRectangleToolHandler
{
    private string defaultActionDisplay = "RECTANGLE_TOOL_ACTION_DISPLAY_DEFAULT";
    public RectangleToolViewModel()
    {
        ActionDisplay = defaultActionDisplay;
    }

    public override string ToolNameLocalizationKey => "RECTANGLE_TOOL";
    public IToolbar Toolbar { get; set; }
    public override LocalizedString Tooltip => new LocalizedString("RECTANGLE_TOOL_TOOLTIP", Shortcut);

    public bool Filled { get; set; } = false;
    public bool DrawSquare { get; private set; } = false;
    public override void ModifierKeyChanged(bool ctrlIsDown, bool shiftIsDown, bool altIsDown)
    {
        if (shiftIsDown)
        {
            DrawSquare = true;
            ActionDisplay = "RECTANGLE_TOOL_ACTION_DISPLAY_SHIFT";
        }
        else
        {
            DrawSquare = false;
            ActionDisplay = defaultActionDisplay;
        }
    }

    public override void UseTool(VecD pos)
    {
        ViewModelMain.Current?.DocumentManagerSubViewModel.ActiveDocument?.Tools.UseRectangleTool();
    }
}
