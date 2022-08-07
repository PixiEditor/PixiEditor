using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Views.UserControls.BrushShapeOverlay;

namespace PixiEditor.ViewModels.SubViewModels.Tools.Tools;

[Command.Tool(Key = Key.R)]
internal class RotateViewportToolViewModel : ToolViewModel
{
    public override BrushShape BrushShape => BrushShape.Hidden;

    public RotateViewportToolViewModel()
    {
        ActionDisplay = "Rotate viewport";
    }

    public override bool HideHighlight => true;

    public override string Tooltip => $"Rotates viewport ({Shortcut})";
}
