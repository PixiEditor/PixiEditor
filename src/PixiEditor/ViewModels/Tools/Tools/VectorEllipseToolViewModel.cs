using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Models.Handlers.Tools;
using PixiEditor.Numerics;
using PixiEditor.UI.Common.Fonts;

namespace PixiEditor.ViewModels.Tools.Tools;

internal class VectorEllipseToolViewModel : ShapeTool, IVectorEllipseToolHandler
{
    private string defaultActionDisplay = "ELLIPSE_TOOL_ACTION_DISPLAY_DEFAULT";
    public override string ToolNameLocalizationKey => "ELLIPSE_TOOL";

    public VectorEllipseToolViewModel()
    {
        ActionDisplay = defaultActionDisplay;
    }

    public override LocalizedString Tooltip => new LocalizedString("ELLIPSE_TOOL_TOOLTIP", Shortcut);
    public bool DrawCircle { get; private set; }

    public override string Icon => PixiPerfectIcons.Circle;

    public override void UseTool(VecD pos)
    {
        ViewModelMain.Current?.DocumentManagerSubViewModel.ActiveDocument?.Tools.UseVectorEllipseTool();
    }
}
