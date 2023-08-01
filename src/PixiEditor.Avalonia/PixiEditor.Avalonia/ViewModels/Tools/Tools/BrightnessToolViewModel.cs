using System.Windows.Input;
using Avalonia.Input;
using ChunkyImageLib.DataHolders;
using PixiEditor.Avalonia.ViewModels;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Containers.Toolbars;
using PixiEditor.Models.Containers.Tools;
using PixiEditor.Models.Enums;
using PixiEditor.Models.Localization;
using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Settings;
using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Toolbars;
using PixiEditor.Views.UserControls.Overlays.BrushShapeOverlay;

namespace PixiEditor.ViewModels.SubViewModels.Tools.Tools;

[Command.Tool(Key = Key.U)]
internal class BrightnessToolViewModel : ToolViewModel, IBrightnessToolHandler
{
    private readonly string defaultActionDisplay = "BRIGHTNESS_TOOL_ACTION_DISPLAY_DEFAULT";
    private int _correctionFactor;
    public override string ToolNameLocalizationKey => "BRIGHTNESS_TOOL";

    public BrightnessToolViewModel()
    {
        ActionDisplay = defaultActionDisplay;
        Toolbar = ToolbarFactory.Create<BrightnessToolViewModel, BasicToolbar>(this);
    }

    public override bool IsErasable => true;

    public IToolbar Toolbar { get; set; }
    public override LocalizedString Tooltip => new LocalizedString("BRIGHTNESS_TOOL_TOOLTIP", Shortcut);

    public override BrushShape BrushShape => BrushShape.Circle;

    BrightnessMode IBrightnessToolHandler.BrightnessMode { get; set; }
    int IBrightnessToolHandler.ToolSize { get; set; }

    [Settings.Inherited]
    public int ToolSize => GetValue<int>();
    
    [Settings.Float("STRENGTH_LABEL", 5)]
    public float CorrectionFactor => GetValue<float>();

    [Settings.Enum("MODE_LABEL")]
    public BrightnessMode BrightnessMode => GetValue<BrightnessMode>();
    
    public bool Darken { get; private set; } = false;

    int IBrightnessToolHandler.CorrectionFactor => (int)CorrectionFactor;

    public override void ModifierKeyChanged(bool ctrlIsDown, bool shiftIsDown, bool altIsDown)
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
}
