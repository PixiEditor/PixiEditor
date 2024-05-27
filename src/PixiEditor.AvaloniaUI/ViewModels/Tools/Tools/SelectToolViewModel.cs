using Avalonia.Input;
using PixiEditor.AvaloniaUI.Models.Commands.Attributes.Commands;
using PixiEditor.AvaloniaUI.Models.Handlers.Tools;
using PixiEditor.AvaloniaUI.Models.Position;
using PixiEditor.AvaloniaUI.ViewModels.Tools.ToolSettings.Toolbars;
using PixiEditor.AvaloniaUI.Views.Overlays.BrushShapeOverlay;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Numerics;

namespace PixiEditor.AvaloniaUI.ViewModels.Tools.Tools;

[Command.Tool(Key = Key.M)]
internal class SelectToolViewModel : ToolViewModel, ISelectToolHandler
{
    private string defaultActionDisplay = "SELECT_TOOL_ACTION_DISPLAY_DEFAULT";
    public override string ToolNameLocalizationKey => "SELECT_TOOL_NAME";

    public override string IconKey => "icon-rectangle-selection";

    public SelectToolViewModel()
    {
        ActionDisplay = defaultActionDisplay;
        Toolbar = ToolbarFactory.Create(this);
        Cursor = new Cursor(StandardCursorType.Cross);
    }

    private SelectionMode KeyModifierselectionMode = SelectionMode.New;
    public SelectionMode ResultingSelectionMode => KeyModifierselectionMode != SelectionMode.New ? KeyModifierselectionMode : SelectMode;

    public override void ModifierKeyChanged(bool ctrlIsDown, bool shiftIsDown, bool altIsDown)
    {
        if (shiftIsDown)
        {
            ActionDisplay = new LocalizedString("SELECT_TOOL_ACTION_DISPLAY_SHIFT");
            KeyModifierselectionMode = SelectionMode.Add;
        }
        else if (ctrlIsDown)
        {
            ActionDisplay = new LocalizedString("SELECT_TOOL_ACTION_DISPLAY_CTRL");
            KeyModifierselectionMode = SelectionMode.Subtract;
        }
        else
        {
            ActionDisplay = defaultActionDisplay;
            KeyModifierselectionMode = SelectionMode.New;
        }
    }

    [Settings.Enum("MODE_LABEL")]
    public SelectionMode SelectMode => GetValue<SelectionMode>();

    [Settings.Enum("SHAPE_LABEL")]
    public SelectionShape SelectShape => GetValue<SelectionShape>();

    public override BrushShape BrushShape => BrushShape.Pixel;

    public override LocalizedString Tooltip => new LocalizedString("SELECT_TOOL_TOOLTIP", Shortcut);

    public override void UseTool(VecD pos)
    {
        ViewModelMain.Current?.DocumentManagerSubViewModel.ActiveDocument?.Tools.UseSelectTool();
    }
}
