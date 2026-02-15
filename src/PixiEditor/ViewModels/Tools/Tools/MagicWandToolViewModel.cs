using Avalonia.Input;
using PixiEditor.ChangeableDocument.Enums;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Vector;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Handlers.Tools;
using PixiEditor.Models.Tools;
using Drawie.Numerics;
using PixiEditor.UI.Common.Fonts;
using PixiEditor.UI.Common.Localization;
using PixiEditor.ViewModels.Tools.ToolSettings.Toolbars;
using PixiEditor.Views.Overlays.BrushShapeOverlay;

namespace PixiEditor.ViewModels.Tools.Tools;

[Command.Tool(Key = Key.W)]
internal class MagicWandToolViewModel : ToolViewModel, IMagicWandToolHandler
{
    public override LocalizedString Tooltip => new LocalizedString("MAGIC_WAND_TOOL_TOOLTIP", Shortcut);
    private LocalizedString defaultActionDisplay => new LocalizedString("MAGIC_WAND_ACTION_DISPLAY");
    public override string ToolNameLocalizationKey => "MAGIC_WAND_TOOL";
    public override Type[]? SupportedLayerTypes { get; } = null;
    private SelectionMode KeyModifierSelectionMode = SelectionMode.New;
    public SelectionMode ResultingSelectionMode => KeyModifierSelectionMode != SelectionMode.New ? KeyModifierSelectionMode : SelectMode;
    [Settings.Enum("MODE_LABEL")]
    public SelectionMode SelectMode => GetValue<SelectionMode>();

    [Settings.Enum("SCOPE_LABEL")]
    public DocumentScope DocumentScope => GetValue<DocumentScope>();

    [Settings.Percent("TOLERANCE_LABEL", ExposedByDefault = false)]
    public float Tolerance => GetValue<float>();
    
    [Settings.Enum("FLOOD_LABEL")]
    public FloodMode FloodMode => GetValue<FloodMode>();

    public override string DefaultIcon => PixiPerfectIcons.MagicWand;

    public MagicWandToolViewModel()
    {
        Toolbar = ToolbarFactory.Create(this);
        ActionDisplay = defaultActionDisplay;
    }

    public override Type LayerTypeToCreateOnEmptyUse { get; } = null;

    public override void UseTool(VecD pos)
    {
        ViewModelMain.Current?.DocumentManagerSubViewModel.ActiveDocument?.Tools.UseMagicWandTool();
    }

    public override void KeyChanged(bool ctrlIsDown, bool shiftIsDown, bool altIsDown, Key argsKey)
    {
        if (shiftIsDown)
        {
            ActionDisplay = new LocalizedString("MAGIC_WAND_ACTION_DISPLAY_SHIFT");
            KeyModifierSelectionMode = SelectionMode.Add;
        }
        else if (ctrlIsDown)
        {
            ActionDisplay = new LocalizedString("MAGIC_WAND_ACTION_DISPLAY_CTRL");
            KeyModifierSelectionMode = SelectionMode.Subtract;
        }
        else
        {
            ActionDisplay = defaultActionDisplay;
            KeyModifierSelectionMode = SelectionMode.New;
        }
    }
}
