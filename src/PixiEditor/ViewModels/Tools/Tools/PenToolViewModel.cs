using Avalonia.Input;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Extensions.CommonApi.UserPreferences.Settings.PixiEditor;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Handlers.Tools;
using PixiEditor.Models.Input;
using PixiEditor.Numerics;
using PixiEditor.UI.Common.Fonts;
using PixiEditor.ViewModels.Tools.ToolSettings.Settings;
using PixiEditor.ViewModels.Tools.ToolSettings.Toolbars;
using PixiEditor.Views.Overlays.BrushShapeOverlay;

namespace PixiEditor.ViewModels.Tools.Tools
{
    [Command.Tool(Key = Key.B)]
    internal class PenToolViewModel : ShapeTool, IPenToolHandler
    {
        private int actualToolSize;

        public override string ToolNameLocalizationKey => "PEN_TOOL";
        public override BrushShape BrushShape => BrushShape.Circle;
        
        public override Type[]? SupportedLayerTypes { get; } = { typeof(IRasterLayerHandler) };

        public PenToolViewModel()
        {
            Cursor = Cursors.PreciseCursor;
            Toolbar = ToolbarFactory.Create<PenToolViewModel, BasicToolbar>(this);
            
            ViewModelMain.Current.ToolsSubViewModel.SelectedToolChanged += SelectedToolChanged;
        }

        public override LocalizedString Tooltip => new LocalizedString("PEN_TOOL_TOOLTIP", Shortcut);

        [Settings.Inherited]
        public int ToolSize => GetValue<int>();

        [Settings.Bool("PIXEL_PERFECT_SETTING", Notify = nameof(PixelPerfectChanged))]
        public bool PixelPerfectEnabled => GetValue<bool>();

        public override string Icon => PixiPerfectIcons.Pen;

        public override Type LayerTypeToCreateOnEmptyUse { get; } = typeof(ImageLayerNode);

        public override void ModifierKeyChanged(bool ctrlIsDown, bool shiftIsDown, bool altIsDown)
        {
            ActionDisplay = new LocalizedString("PEN_TOOL_ACTION_DISPLAY", Shortcut);
        }

        public override void UseTool(VecD pos)
        {
            ViewModelMain.Current?.DocumentManagerSubViewModel.ActiveDocument?.Tools.UsePenTool();
        }

        private void SelectedToolChanged(object sender, SelectedToolEventArgs e)
        {
            if (e.NewTool == this && PixelPerfectEnabled)
            {
                var toolbar = (BasicToolbar)Toolbar;
                var setting = (SizeSettingViewModel)toolbar.Settings.First(x => x.Name == "ToolSize");
                setting.Value = 1;
            }
            
            if (!PixiEditorSettings.Tools.EnableSharedToolbar.Value)
            {
                return;
            }

            if (e.OldTool is not { Toolbar: BasicToolbar oldToolbar })
            {
                return;
            }
            
            var oldSetting = (SizeSettingViewModel)oldToolbar.Settings[0];
            actualToolSize = oldSetting.Value;
        }

        public override void OnDeselecting()
        {
            if (!PixelPerfectEnabled)
            {
                return;
            }

            var toolbar = (BasicToolbar)Toolbar;
            var setting = (SizeSettingViewModel)toolbar.Settings[0];
            setting.Value = actualToolSize;
        }

        private void PixelPerfectChanged()
        {
            var toolbar = (BasicToolbar)Toolbar;
            var setting = (SizeSettingViewModel)toolbar.Settings[0];

            setting.IsEnabled = !PixelPerfectEnabled;

            if (PixelPerfectEnabled)
            {
                actualToolSize = ToolSize;
                setting.Value = 1;
            }
            else
            {
                setting.Value = actualToolSize;
            }
        }
    }
}
