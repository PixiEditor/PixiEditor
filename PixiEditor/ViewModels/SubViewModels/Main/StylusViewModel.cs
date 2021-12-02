using System.Windows;
using System.Windows.Input;
using PixiEditor.Models.Tools;
using PixiEditor.Models.Tools.Tools;
using PixiEditor.Models.UserPreferences;

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

        public StylusViewModel()
            : this(null)
        {
        }

        public StylusViewModel(ViewModelMain owner)
            : base(owner)
        {
        }

        public void SetOwner(ViewModelMain owner)
        {
            if (Owner is not null)
            {
                throw new System.Exception($"{nameof(StylusViewModel)} already has an owner");
            }
            else if (owner is null)
            {
                return;
            }

            Owner = owner;

            // TODO: Only capture it on the Drawing View Port
            Window mw = Application.Current.MainWindow;

            mw.PreviewStylusButtonDown += Mw_StylusButtonDown;
            mw.PreviewStylusButtonUp += Mw_StylusButtonUp;
            mw.PreviewStylusSystemGesture += Mw_PreviewStylusSystemGesture;

            isPenModeEnabled = IPreferences.Current.GetLocalPreference<bool>(nameof(IsPenModeEnabled));
            Owner.ToolsSubViewModel.AddPropertyChangedCallback(nameof(ToolsViewModel.ActiveTool), UpdateUseTouchGesture);

            UpdateUseTouchGesture();
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

        private void Mw_PreviewStylusSystemGesture(object sender, StylusSystemGestureEventArgs e)
        {
            if (e.SystemGesture == SystemGesture.Drag || e.SystemGesture == SystemGesture.Tap)
            {
                return;
            }

            e.Handled = true;
        }

        private void Mw_StylusButtonDown(object sender, StylusButtonEventArgs e)
        {
            e.Handled = true;

            if (e.StylusButton.Guid == StylusPointProperties.TipButton.Id && e.Inverted)
            {
                PreviousTool = Owner.ToolsSubViewModel.ActiveTool;
                Owner.ToolsSubViewModel.SetActiveTool<EraserTool>();
                ToolSetByStylus = true;
            }
        }

        private void Mw_StylusButtonUp(object sender, StylusButtonEventArgs e)
        {
            e.Handled = true;

            if (ToolSetByStylus && e.StylusButton.Guid == StylusPointProperties.TipButton.Id && e.Inverted)
            {
                Owner.ToolsSubViewModel.SetActiveTool(PreviousTool);
            }
        }
    }
}