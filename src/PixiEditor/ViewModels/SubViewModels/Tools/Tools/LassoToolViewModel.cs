using System.Windows.Input;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Localization;
using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Toolbars;
using PixiEditor.Views.UserControls.Overlays.BrushShapeOverlay;

namespace PixiEditor.ViewModels.SubViewModels.Tools.Tools;

[Command.ToolAttribute(Key = Key.Q)]
internal class LassoToolViewModel : ToolViewModel
{
    private string defaultActionDisplay = "LASSO_TOOL_ACTION_DISPLAY_DEFAULT";

    public LassoToolViewModel()
    {
        Toolbar = ToolbarFactory.Create(this);
        ActionDisplay = defaultActionDisplay;
    }

    private SelectionMode modifierKeySelectionMode = SelectionMode.New;
    public SelectionMode ResultingSelectionMode => modifierKeySelectionMode != SelectionMode.New ? modifierKeySelectionMode : SelectMode;

    public override void ModifierKeyChanged(bool ctrlIsDown, bool shiftIsDown, bool altIsDown)
    {
        if (shiftIsDown)
        {
            ActionDisplay = "LASSO_TOOL_ACTION_DISPLAY_SHIFT";
            modifierKeySelectionMode = SelectionMode.Add;
        }
        else if (ctrlIsDown)
        {
            ActionDisplay = "LASSO_TOOL_ACTION_DISPLAY_CTRL";
            modifierKeySelectionMode = SelectionMode.Subtract;
        }
        else
        {
            ActionDisplay = defaultActionDisplay;
            modifierKeySelectionMode = SelectionMode.New;
        }
    }

    public override LocalizedString Tooltip => new LocalizedString("LASSO_TOOL_TOOLTIP", Shortcut);

    public override string ToolNameLocalizationKey => "LASSO_TOOL";
    public override BrushShape BrushShape => BrushShape.Pixel;

    [Settings.Enum("MODE_LABEL")]
    public SelectionMode SelectMode => GetValue<SelectionMode>();
    
    public override void OnLeftMouseButtonDown(VecD pos)
    {
        ViewModelMain.Current?.DocumentManagerSubViewModel.ActiveDocument?.Tools.UseLassoTool();
    }
}
