using System.Windows.Input;
using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Localization;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Enums;
using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Toolbars;
using PixiEditor.Views.UserControls.Overlays.BrushShapeOverlay;

namespace PixiEditor.ViewModels.SubViewModels.Tools.Tools;

[Command.Tool(Key = Key.M)]
internal class SelectToolViewModel : ToolViewModel
{
    private string defaultActionDisplay = new LocalizedString("SELECT_TOOL_ACTION_DISPLAY_DEFAULT");
    public override string ToolNameLocalizationKey => "SELECT_TOOL_NAME";
    public SelectToolViewModel()
    {
        ActionDisplay = defaultActionDisplay;
        Toolbar = ToolbarFactory.Create<SelectToolViewModel>();
        Cursor = Cursors.Cross;
    }

    private SelectionMode modifierKeySelectionMode = SelectionMode.New;
    public SelectionMode ResultingSelectionMode => modifierKeySelectionMode != SelectionMode.New ? modifierKeySelectionMode : SelectMode;

    public override void ModifierKeyChanged(bool ctrlIsDown, bool shiftIsDown, bool altIsDown)
    {
        if (shiftIsDown)
        {
            ActionDisplay = new LocalizedString("SELECT_TOOL_ACTION_DISPLAY_SHIFT");
            modifierKeySelectionMode = SelectionMode.Add;
        }
        else if (ctrlIsDown)
        {
            ActionDisplay = new LocalizedString("SELECT_TOOL_ACTION_DISPLAY_CTRL");
            modifierKeySelectionMode = SelectionMode.Subtract;
        }
        else
        {
            ActionDisplay = defaultActionDisplay;
            modifierKeySelectionMode = SelectionMode.New;
        }
    }

    [Settings.Enum("MODE_LABEL")]
    public SelectionMode SelectMode => GetValue<SelectionMode>();

    [Settings.Enum("SHAPE_LABEL")]
    public SelectionShape SelectShape => GetValue<SelectionShape>();

    public override BrushShape BrushShape => BrushShape.Pixel;

    public override LocalizedString Tooltip => new LocalizedString("SELECT_TOOL_TOOLTIP", Shortcut);

    public override void OnLeftMouseButtonDown(VecD pos)
    {
        ViewModelMain.Current?.DocumentManagerSubViewModel.ActiveDocument?.Tools.UseSelectTool();
    }
}
