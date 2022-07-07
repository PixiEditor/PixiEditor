using GalaSoft.MvvmLight.CommandWpf;
using PixiEditor.Models.Tools;
using PixiEditor.Models.Tools.Tools;
using PixiEditor.Models.UserPreferences;
using System.Windows.Input;
using PixiEditor.Models.Commands.Attributes;

namespace PixiEditor.ViewModels.SubViewModels.Main
{
    public class StylusViewModel : SubViewModel<ViewModelMain>
    {
        private bool isPenModeEnabled;
        private bool useTouchGestures;

        public bool ToolSetByStylus { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether touch gestures are enabled even when the MoveViewportTool and ZoomTool are not selected.
        /// </summary>
        public bool IsPenModeEnabled
        {
            get => isPenModeEnabled;
            set
            {
                if (SetProperty(ref isPenModeEnabled, value))
                {
                    IPreferences.Current.UpdateLocalPreference(nameof(IsPenModeEnabled), value);
                    UpdateUseTouchGesture();
                }
            }
        }

        public bool UseTouchGestures
        {
            get => useTouchGestures;
            set => SetProperty(ref useTouchGestures, value);
        }

        private Tool PreviousTool { get; set; }

        public RelayCommand<StylusButtonEventArgs> StylusDownCommand { get; }

        public RelayCommand<StylusButtonEventArgs> StylusUpCommand { get; }

        public RelayCommand<StylusEventArgs> StylusOutOfRangeCommand { get; }

        public RelayCommand<StylusSystemGestureEventArgs> StylusGestureCommand { get; }

        public StylusViewModel(ViewModelMain owner)
            : base(owner)
        {
            StylusDownCommand = new(StylusDown);
            StylusUpCommand = new(StylusUp);
            StylusOutOfRangeCommand = new(StylusOutOfRange);
            StylusGestureCommand = new(StylusSystemGesture);

            isPenModeEnabled = IPreferences.Current.GetLocalPreference<bool>(nameof(IsPenModeEnabled));
            Owner.ToolsSubViewModel.AddPropertyChangedCallback(nameof(ToolsViewModel.ActiveTool), UpdateUseTouchGesture);

            UpdateUseTouchGesture();
        }

        [Command.Basic("PixiEditor.Stylus.TogglePenMode", "Toggle Pen Mode", "Toggle Pen Mode")]
        public void TogglePenMode()
        {
            IsPenModeEnabled = !IsPenModeEnabled;
        }

        private void UpdateUseTouchGesture()
        {
            if (Owner.ToolsSubViewModel.ActiveTool is not (MoveViewportTool or ZoomTool))
            {
                UseTouchGestures = IsPenModeEnabled;
            }
            else
            {
                UseTouchGestures = true;
            }
        }

        private void StylusOutOfRange(StylusEventArgs e)
        {
            Owner.BitmapManager.UpdateHighlightIfNecessary(true);
        }

        private void StylusSystemGesture(StylusSystemGestureEventArgs e)
        {
            if (e.SystemGesture == SystemGesture.Drag || e.SystemGesture == SystemGesture.Tap)
            {
                return;
            }

            e.Handled = true;
        }

        private void StylusDown(StylusButtonEventArgs e)
        {
            e.Handled = true;

            if (e.StylusButton.Guid == StylusPointProperties.TipButton.Id && e.Inverted)
            {
                PreviousTool = Owner.ToolsSubViewModel.ActiveTool;
                Owner.ToolsSubViewModel.SetActiveTool<EraserTool>();
                ToolSetByStylus = true;
            }
        }

        private void StylusUp(StylusButtonEventArgs e)
        {
            e.Handled = true;

            if (ToolSetByStylus && e.StylusButton.Guid == StylusPointProperties.TipButton.Id && e.Inverted)
            {
                Owner.ToolsSubViewModel.SetActiveTool(PreviousTool);
            }
        }
    }
}
