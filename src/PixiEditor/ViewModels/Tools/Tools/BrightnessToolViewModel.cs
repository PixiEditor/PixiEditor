using Avalonia.Input;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Vector;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Handlers.Tools;
using PixiEditor.Models.Tools;
using Drawie.Numerics;
using PixiEditor.Models.Handlers.Toolbars;
using PixiEditor.UI.Common.Fonts;
using PixiEditor.UI.Common.Localization;
using PixiEditor.ViewModels.Tools.ToolSettings.Toolbars;
using PixiEditor.Views.Overlays.BrushShapeOverlay;

namespace PixiEditor.ViewModels.Tools.Tools;

[Command.Tool(Key = Key.U)]
internal class BrightnessToolViewModel : ToolViewModel, IBrightnessToolHandler
{
    private readonly string defaultActionDisplay = "BRIGHTNESS_TOOL_ACTION_DISPLAY_DEFAULT";
    private int _correctionFactor;
    public override string ToolNameLocalizationKey => "BRIGHTNESS_TOOL";

    public BrightnessToolViewModel()
    {
        ActionDisplay = defaultActionDisplay;
        Toolbar = ToolbarFactory.Create<BrightnessToolViewModel, BrightnessToolbar>(this);
    }

    public override bool IsErasable => true;
    public override LocalizedString Tooltip => new LocalizedString("BRIGHTNESS_TOOL_TOOLTIP", Shortcut);

    public override string DefaultIcon => PixiPerfectIcons.Sun;

    public override Type[]? SupportedLayerTypes { get; } =
    {
        typeof(IRasterLayerHandler)
    };

    BrightnessMode IBrightnessToolHandler.BrightnessMode
    {
        get => BrightnessMode;
    }

    [Settings.Inherited]
    public double ToolSize => GetValue<double>();
    
    [Settings.Float("STRENGTH_LABEL", 5, 0, 50)]
    public float CorrectionFactor => GetValue<float>();

    [Settings.Enum("MODE_LABEL")]
    public BrightnessMode BrightnessMode => GetValue<BrightnessMode>();

    public bool Darken { get; private set; } = false;

    float IBrightnessToolHandler.CorrectionFactor => CorrectionFactor;

    public override Type LayerTypeToCreateOnEmptyUse { get; } = typeof(ImageLayerNode);

    public override void KeyChanged(bool ctrlIsDown, bool shiftIsDown, bool altIsDown, Key argsKey)
    {
        if (!ctrlIsDown)
        {
            ActionDisplay = defaultActionDisplay;
            Darken = false;
        }
        else
        {
            ActionDisplay = "BRIGHTNESS_TOOL_ACTION_DISPLAY_CTRL";
            Darken = true;
        }
    }

    public override void UseTool(VecD pos)
    {
        ViewModelMain.Current?.DocumentManagerSubViewModel.ActiveDocument?.Tools.UseBrightnessTool();
    }

    private void BrushShapeChanged()
    {
        OnPropertyChanged(nameof(FinalBrushShape));
    }
}
