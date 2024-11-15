using Avalonia.Input;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Extensions.CommonApi.UserPreferences.Settings.PixiEditor;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Handlers.Tools;
using PixiEditor.Models.Input;
using Drawie.Numerics;
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
        public override BrushShape BrushShape => BrushShapeSetting;
        
        public override Type[]? SupportedLayerTypes { get; } = { typeof(IRasterLayerHandler) };

        public PenToolViewModel()
        {
            Cursor = Cursors.PreciseCursor;
            Toolbar = ToolbarFactory.Create<PenToolViewModel, PenToolbar>(this);
            
            ViewModelMain.Current.ToolsSubViewModel.SelectedToolChanged += SelectedToolChanged;
        }

        public override LocalizedString Tooltip => new LocalizedString("PEN_TOOL_TOOLTIP", Shortcut);

        [Settings.Inherited]
        public int ToolSize => GetValue<int>();

        [Settings.Bool("PIXEL_PERFECT_SETTING", Notify = nameof(PixelPerfectChanged), ExposedByDefault = false)]
        public bool PixelPerfectEnabled => GetValue<bool>();
        
        [Settings.Enum("BRUSH_SHAPE_SETTING", BrushShape.CirclePixelated, ExposedByDefault = false, Notify = nameof(BrushShapeChanged))]
        public BrushShape BrushShapeSetting => GetValue<BrushShape>();

        public override string DefaultIcon => PixiPerfectIcons.Pen;

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

        public override void OnDeselecting(bool transient)
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
        
        private void BrushShapeChanged()
        {
            OnPropertyChanged(nameof(BrushShape));
        }
    }
}
