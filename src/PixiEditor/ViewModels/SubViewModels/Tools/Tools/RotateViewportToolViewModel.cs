using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Localization;
using PixiEditor.Views.UserControls.Overlays.BrushShapeOverlay;

namespace PixiEditor.ViewModels.SubViewModels.Tools.Tools;

[Command.Tool(Key = Key.N)]
internal class RotateViewportToolViewModel : ToolViewModel
{
    public override string ToolNameLocalizationKey => "ROTATE_VIEWPORT_TOOL";
    public override BrushShape BrushShape => BrushShape.Hidden;
    public override bool HideHighlight => true;
    public override LocalizedString Tooltip => new LocalizedString("ROTATE_VIEWPORT_TOOLTIP", Shortcut);

    public RotateViewportToolViewModel()
    {
    }

    public override void OnSelected()
    {
        ActionDisplay = new LocalizedString("ROTATE_VIEWPORT_ACTION_DISPLAY");
    }
}
