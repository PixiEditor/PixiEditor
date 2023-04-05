using System.Windows.Input;
using ChunkyImageLib.DataHolders;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Localization;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Enums;
using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Toolbars;
using PixiEditor.Views.UserControls.Overlays.BrushShapeOverlay;

namespace PixiEditor.ViewModels.SubViewModels.Tools.Tools;

[Command.Tool(Key = Key.O, Transient = Key.LeftAlt)]
internal class ColorPickerToolViewModel : ToolViewModel
{
    private readonly LocalizedString defaultActionDisplay = "COLOR_PICKER_ACTION_DISPLAY_DEFAULT";

    public override bool HideHighlight => true;

    public override string ToolNameLocalizationKey => "COLOR_PICKER_TOOL";
    public override BrushShape BrushShape => BrushShape.Pixel;

    public override LocalizedString Tooltip => new("COLOR_PICKER_TOOLTIP", Shortcut);

    private bool pickFromCanvas = true;
    public bool PickFromCanvas
    {
        get => pickFromCanvas; 
        private set => SetProperty(ref pickFromCanvas, value);
    }
    
    private bool pickFromReferenceLayer = true;
    public bool PickFromReferenceLayer
    {
        get => pickFromReferenceLayer; 
        private set => SetProperty(ref pickFromReferenceLayer, value);
    }

    [Settings.Enum("SCOPE_LABEL", DocumentScope.AllLayers)]
    public DocumentScope Mode => GetValue<DocumentScope>();

    public ColorPickerToolViewModel()
    {
        ActionDisplay = defaultActionDisplay;
        Toolbar = ToolbarFactory.Create<ColorPickerToolViewModel, EmptyToolbar>();
    }

    public override void OnLeftMouseButtonDown(VecD pos)
    {
        ViewModelMain.Current?.DocumentManagerSubViewModel.ActiveDocument?.Tools.UseColorPickerTool();
    }

    public override void ModifierKeyChanged(bool ctrlIsDown, bool shiftIsDown, bool altIsDown)
    {
        if (ctrlIsDown)
        {
            PickFromCanvas = false;
            PickFromReferenceLayer = true;
            ActionDisplay = "COLOR_PICKER_ACTION_DISPLAY_CTRL";
        }
        else if (shiftIsDown)
        {
            PickFromCanvas = true;
            PickFromReferenceLayer = false;
            ActionDisplay = "COLOR_PICKER_ACTION_DISPLAY_SHIFT";
            return;
        }
        else
        {
            PickFromCanvas = true;
            PickFromReferenceLayer = true;
            ActionDisplay = defaultActionDisplay;
        }
    }
}
