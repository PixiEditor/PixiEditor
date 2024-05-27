using Avalonia.Input;
using PixiEditor.AvaloniaUI.Models.Commands.Attributes.Commands;
using PixiEditor.AvaloniaUI.Models.Handlers.Tools;
using PixiEditor.AvaloniaUI.Models.Tools;
using PixiEditor.AvaloniaUI.ViewModels.Tools.ToolSettings.Toolbars;
using PixiEditor.AvaloniaUI.Views.Overlays.BrushShapeOverlay;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Numerics;

namespace PixiEditor.AvaloniaUI.ViewModels.Tools.Tools;

[Command.Tool(Key = Key.W)]
internal class MagicWandToolViewModel : ToolViewModel, IMagicWandToolHandler
{
    public override LocalizedString Tooltip => new LocalizedString("MAGIC_WAND_TOOL_TOOLTIP", Shortcut);

    public override string ToolNameLocalizationKey => "MAGIC_WAND_TOOL";
    public override BrushShape BrushShape => BrushShape.Pixel;

    [Settings.Enum("MODE_LABEL")]
    public SelectionMode SelectMode => GetValue<SelectionMode>();

    [Settings.Enum("SCOPE_LABEL")]
    public DocumentScope DocumentScope => GetValue<DocumentScope>();

    public override string IconKey => "icon-magic-wand";

    public MagicWandToolViewModel()
    {
        Toolbar = ToolbarFactory.Create(this);
        ActionDisplay = "MAGIC_WAND_ACTION_DISPLAY";
    }
    
    public override void UseTool(VecD pos)
    {
        ViewModelMain.Current?.DocumentManagerSubViewModel.ActiveDocument?.Tools.UseMagicWandTool();
    }
}
