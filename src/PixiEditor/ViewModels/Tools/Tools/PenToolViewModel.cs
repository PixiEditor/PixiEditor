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
using PixiEditor.ViewModels.Document.Blackboard;
using PixiEditor.ViewModels.Tools.ToolSettings.Settings;
using PixiEditor.ViewModels.Tools.ToolSettings.Toolbars;
using PixiEditor.Views.Blackboard;
using PixiEditor.Views.Overlays.BrushShapeOverlay;

namespace PixiEditor.ViewModels.Tools.Tools
{
    [Command.Tool(Key = Key.B)]
    internal class PenToolViewModel : BrushBasedToolViewModel, IPenToolHandler
    {
        private double actualToolSize = 1;

        public override string ToolNameLocalizationKey => "PEN_TOOL";

        public PenToolViewModel()
        {
            ViewModelMain.Current.ToolsSubViewModel.SelectedToolChanged += SelectedToolChanged;
        }

        public override LocalizedString Tooltip => new LocalizedString("PEN_TOOL_TOOLTIP", Shortcut);

        [Settings.Bool("PIXEL_PERFECT_SETTING", Notify = nameof(PixelPerfectChanged), ExposedByDefault = false)]
        public bool PixelPerfectEnabled => GetValue<bool>();

        public override string DefaultIcon => PixiPerfectIcons.Pen;

        public override void KeyChanged(bool ctrlIsDown, bool shiftIsDown, bool altIsDown, Key argsKey)
        {
            ActionDisplay = new LocalizedString("PEN_TOOL_ACTION_DISPLAY", Shortcut);
        }

        protected override Toolbar CreateToolbar()
        {
            return ToolbarFactory.Create<PenToolViewModel, BrushToolbar>(this);
        }

        protected override void SwitchToTool()
        {
            ViewModelMain.Current?.DocumentManagerSubViewModel.ActiveDocument?.Tools.UsePenTool();
        }

        private void SelectedToolChanged(object sender, SelectedToolEventArgs e)
        {
            if (e.NewTool == this && PixelPerfectEnabled)
            {
                var toolbar = (BrushToolbar)Toolbar;
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

            if (e.OldTool is not { Toolbar: BrushToolbar oldToolbar })
            {
                return;
            }

            var oldSetting = oldToolbar.Settings.FirstOrDefault(x => x.Name == nameof(oldToolbar.ToolSize));
            if (oldSetting is null)
            {
                return;
            }

            if (oldSetting.Value is int intValue)
            {
                actualToolSize = intValue;
            }
            else if (oldSetting.Value is double doubleValue)
            {
                actualToolSize = (int)doubleValue;
            }
        }

        protected override void OnDeselecting(bool transient)
        {
            base.OnDeselecting(transient);

            actualToolSize = ToolSize;
            if (!PixelPerfectEnabled)
            {
                return;
            }

            var toolbar = (BrushToolbar)Toolbar;
            var setting = toolbar.Settings.FirstOrDefault(x => x.Name == nameof(toolbar.ToolSize));
            if (setting is SizeSettingViewModel sizeSetting)
            {
                sizeSetting.Value = actualToolSize;
            }
        }

        private void PixelPerfectChanged()
        {
            var toolbar = (BrushToolbar)Toolbar;
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
    }
}
