using Avalonia.Input;
using Drawie.Backend.Core.Vector;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.Extensions.CommonApi.UserPreferences.Settings.PixiEditor;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Handlers.Tools;
using PixiEditor.Models.Input;
using Drawie.Numerics;
using PixiEditor.Models.Handlers.Toolbars;
using PixiEditor.UI.Common.Fonts;
using PixiEditor.UI.Common.Localization;
using PixiEditor.ViewModels.Tools.ToolSettings.Settings;
using PixiEditor.ViewModels.Tools.ToolSettings.Toolbars;
using PixiEditor.Views.Overlays.BrushShapeOverlay;

namespace PixiEditor.ViewModels.Tools.Tools
{
    [Command.Tool(Key = Key.B)]
    internal class PenToolViewModel : ShapeTool, IPenToolHandler
    {
        private double actualToolSize = 1;

        public override string ToolNameLocalizationKey => "PEN_TOOL";

            /*
            PaintShape == PaintBrushShape.Square ? BrushShape.Square : BrushShapeSetting;
            */

        public override Type[]? SupportedLayerTypes { get; } = { typeof(IRasterLayerHandler) };

        public PenToolViewModel()
        {
            Cursor = Cursors.PreciseCursor;
            Toolbar = ToolbarFactory.Create<PenToolViewModel, PenToolbar>(this);

            ViewModelMain.Current.ToolsSubViewModel.SelectedToolChanged += SelectedToolChanged;
        }

        public override LocalizedString Tooltip => new LocalizedString("PEN_TOOL_TOOLTIP", Shortcut);

        [Settings.Inherited]
        public double ToolSize => GetValue<double>();

        [Settings.Bool("PIXEL_PERFECT_SETTING", Notify = nameof(PixelPerfectChanged), ExposedByDefault = false)]
        public bool PixelPerfectEnabled => GetValue<bool>();
        
        [Settings.Enum("BRUSH_SHAPE_SETTING", BrushShape.CirclePixelated, ExposedByDefault = false, Notify = nameof(BrushShapeChanged))]
        public BrushShape BrushShapeSetting
        {
            get
            {
                return GetValue<BrushShape>();
            }
            set
            {
                SetValue(value);
                OnPropertyChanged(nameof(FinalBrushShape));
            }
        }

        [Settings.Inherited(Notify = nameof(PenShapeChanged))]
        public PaintBrushShape PaintShape => GetValue<PaintBrushShape>();

        public override string DefaultIcon => PixiPerfectIcons.Pen;

        public override Type LayerTypeToCreateOnEmptyUse { get; } = typeof(ImageLayerNode);

        public override void KeyChanged(bool ctrlIsDown, bool shiftIsDown, bool altIsDown, Key argsKey)
        {
            ActionDisplay = new LocalizedString("PEN_TOOL_ACTION_DISPLAY", Shortcut);
        }

        public override void UseTool(VecD pos)
        {
            ViewModelMain.Current?.DocumentManagerSubViewModel.ActiveDocument?.Tools.UsePenTool();
        }

        public void OnToolSelected(bool restoring)
        {
            ViewModelMain.Current?.DocumentManagerSubViewModel.ActiveDocument?.Tools.UsePenTool();
        }

        public void OnPostUndoInlet()
        {
            ViewModelMain.Current?.DocumentManagerSubViewModel.ActiveDocument?.Tools.UsePenTool();
        }

        public override void OnPostRedoInlet()
        {
            ViewModelMain.Current?.DocumentManagerSubViewModel.ActiveDocument?.Tools.UsePenTool();
        }

        private void SelectedToolChanged(object sender, SelectedToolEventArgs e)
        {
            if (e.NewTool == this && PixelPerfectEnabled)
            {
                var toolbar = (PenToolbar)Toolbar;
                var setting = toolbar.Settings.FirstOrDefault(x => x.Name == nameof(toolbar.ToolSize));
                if (setting is SizeSettingViewModel sizeSetting)
                {
                    sizeSetting.Value = 1d;
                }
            }
            
            if (!PixiEditorSettings.Tools.EnableSharedToolbar.Value)
            {
                return;
            }

            if (e.OldTool is not { Toolbar: PenToolbar oldToolbar })
            {
                return;
            }
            
            var oldSetting = oldToolbar.Settings.FirstOrDefault(x => x.Name == nameof(oldToolbar.ToolSize));
            if (oldSetting is null)
            {
                return;
            }
            
            if(oldSetting.Value is int intValue)
            {
                actualToolSize = intValue;
            }
            else if(oldSetting.Value is double doubleValue)
            {
                actualToolSize = (int)doubleValue;
            }
        }

        protected override void OnDeselecting(bool transient)
        {
            if (!PixelPerfectEnabled)
            {
                return;
            }

            var toolbar = (PenToolbar)Toolbar;
            var setting = toolbar.Settings.FirstOrDefault(x => x.Name == nameof(toolbar.ToolSize));
            if(setting is SizeSettingViewModel sizeSetting)
            {
                sizeSetting.Value = actualToolSize;
            }
        }

        private void PixelPerfectChanged()
        {
            var toolbar = (PenToolbar)Toolbar;
            var setting = toolbar.Settings.FirstOrDefault(x => x.Name == nameof(toolbar.ToolSize));

            if (setting is SizeSettingViewModel sizeSettingViewModel)
            {
                sizeSettingViewModel.IsEnabled = !PixelPerfectEnabled;

                if (PixelPerfectEnabled)
                {
                    actualToolSize = ToolSize;
                    sizeSettingViewModel.Value = 1d;
                }
                else
                {
                    sizeSettingViewModel.Value = actualToolSize;
                }
            }
        }

        private void BrushShapeChanged()
        {
            OnPropertyChanged(nameof(FinalBrushShape));
        }

        private void PenShapeChanged()
        {
            OnPropertyChanged(nameof(PaintShape));
            OnPropertyChanged(nameof(FinalBrushShape));
        }
    }
}
