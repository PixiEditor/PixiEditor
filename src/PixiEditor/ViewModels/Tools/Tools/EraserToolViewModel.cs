using Avalonia.Input;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Handlers.Tools;
using PixiEditor.UI.Common.Localization;
using PixiEditor.ViewModels.Tools.ToolSettings.Toolbars;

namespace PixiEditor.ViewModels.Tools.Tools;

[Command.Tool(Key = Key.E)]
internal class EraserToolViewModel : BrushBasedToolViewModel, IEraserToolHandler
{
    public EraserToolViewModel()
    {
        ActionDisplay = "ERASER_TOOL_ACTION_DISPLAY";
        //Toolbar = ToolbarFactory.Create<EraserToolViewModel, PenToolbar>(this);
    }

    public override bool IsErasable => true;

    public override string ToolNameLocalizationKey => "ERASER_TOOL";
    public override string DefaultIcon => PixiPerfectIcons.Eraser;

    public override LocalizedString Tooltip => new LocalizedString("ERASER_TOOL_TOOLTIP", Shortcut);

    protected override Toolbar CreateToolbar()
    {
        return ToolbarFactory.Create<EraserToolViewModel, BrushToolbar>(this);
    }

    protected override void SwitchToTool()
    {
        ViewModelMain.Current?.DocumentManagerSubViewModel.ActiveDocument?.Tools.UseEraserTool();
        if (!createdBrushSettings)
        {
            AddBrushShapeSettings();
        }
    }
}
