using System.Windows.Input;
using System.Windows.Media;
using ChunkyImageLib.DataHolders;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Localization;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Settings;
using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Toolbars;
using PixiEditor.Views.UserControls.Overlays.BrushShapeOverlay;

namespace PixiEditor.ViewModels.SubViewModels.Tools.Tools
{
    [Command.Tool(Key = Key.B)]
    internal class PenToolViewModel : ShapeTool
    {
        public override string ToolNameLocalizationKey => "PEN_TOOL";
        public override BrushShape BrushShape => BrushShape.Circle;
        public PenToolViewModel()
        {
            Cursor = Cursors.Pen;
            Toolbar = ToolbarFactory.Create<PenToolViewModel, BasicToolbar>();
        }

        public override LocalizedString Tooltip => new LocalizedString("PEN_TOOL_TOOLTIP", Shortcut);

        [Settings.Inherited]
        public int ToolSize => GetValue<int>();

        [Settings.Bool("PIXEL_PERFECT_SETTING")]
        public bool PixelPerfectEnabled => GetValue<bool>();

        public override void UpdateActionDisplay(bool ctrlIsDown, bool shiftIsDown, bool altIsDown)
        {
            ActionDisplay = new LocalizedString("PEN_TOOL_TOOLTIP", Shortcut);
        }

        public override void OnLeftMouseButtonDown(VecD pos)
        {
            ViewModelMain.Current?.DocumentManagerSubViewModel.ActiveDocument?.Tools.UsePenTool();
        }
    }
}
