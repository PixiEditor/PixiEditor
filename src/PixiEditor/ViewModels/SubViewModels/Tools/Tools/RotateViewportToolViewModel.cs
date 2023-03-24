using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using PixiEditor.Localization;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Views.UserControls.Overlays.BrushShapeOverlay;

namespace PixiEditor.ViewModels.SubViewModels.Tools.Tools;

[Command.Tool(Key = Key.N)]
internal class RotateViewportToolViewModel : ToolViewModel
{
    public override BrushShape BrushShape => BrushShape.Hidden;
    public override bool HideHighlight => true;
    public override LocalizedString Tooltip => new LocalizedString("ROTATE_VIEWPORT_TOOLTIP", Shortcut);

    public RotateViewportToolViewModel()
    {
        ActionDisplay = new LocalizedString("ROTATE_VIEWPORT_ACTION_DISPLAY");
    }

    public override void OnSelected() { }
}
