using System.Windows.Input;
using System.Windows.Media;
using ChunkyImageLib.DataHolders;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Settings;
using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Toolbars;
using PixiEditor.Views.UserControls.BrushShapeOverlay;

namespace PixiEditor.ViewModels.SubViewModels.Tools.Tools
{
    [Command.Tool(Key = Key.B)]
    internal class PenToolViewModel : ShapeTool
    {
        public override BrushShape BrushShape => BrushShape.Circle;
        public PenToolViewModel()
        {
            Cursor = Cursors.Pen;
            ActionDisplay = "Click and move to draw.";
            Toolbar = ToolbarFactory.Create<PenToolViewModel, BasicToolbar>();
        }

        public override string Tooltip => $"Pen. ({Shortcut})";

        [Settings.Inherited]
        public int ToolSize => GetValue<int>();

        [Settings.Bool("Pixel perfect")]
        public bool PixelPerfectEnabled => GetValue<bool>();

        public override void OnLeftMouseButtonDown(VecD pos)
        {
            ViewModelMain.Current?.DocumentManagerSubViewModel.ActiveDocument?.Tools.UsePenTool();
        }
    }
}
