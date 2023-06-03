using System.Windows.Input;
using PixiEditor.Extensions.Common.UserPreferences;
using PixiEditor.Models.Commands.Attributes;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.ViewModels.SubViewModels.Tools;
using PixiEditor.ViewModels.SubViewModels.Tools.Tools;

namespace PixiEditor.ViewModels.SubViewModels.Main;

[Command.Group("PixiEditor.Stylus", "STYLUS")]
internal class StylusViewModel : SubViewModel<ViewModelMain>
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

    private ToolViewModel PreviousTool { get; set; }

    public StylusViewModel(ViewModelMain owner)
        : base(owner)
    {
        isPenModeEnabled = IPreferences.Current.GetLocalPreference<bool>(nameof(IsPenModeEnabled));
        Owner.ToolsSubViewModel.AddPropertyChangedCallback(nameof(ToolsViewModel.ActiveTool), UpdateUseTouchGesture);

        UpdateUseTouchGesture();
    }

    [Command.Basic("PixiEditor.Stylus.TogglePenMode", "TOGGLE_PEN_MODE", "TOGGLE_PEN_MODE", IconPath = "penMode.png")]
    public void TogglePenMode()
    {
        IsPenModeEnabled = !IsPenModeEnabled;
    }

    private void UpdateUseTouchGesture()
    {
        UseTouchGestures = Owner.ToolsSubViewModel.ActiveTool is MoveViewportToolViewModel or ZoomToolViewModel || IsPenModeEnabled;
    }

    [Command.Internal("PixiEditor.Stylus.StylusOutOfRange")]
    public void StylusOutOfRange(StylusEventArgs e)
    {
        //Owner.BitmapManager.UpdateHighlightIfNecessary(true);
    }

    [Command.Internal("PixiEditor.Stylus.StylusSystemGesture")]
    public void StylusSystemGesture(StylusSystemGestureEventArgs e)
    {
        if (e.SystemGesture is SystemGesture.Drag or SystemGesture.Tap)
        {
            return;
        }

        e.Handled = true;
    }

    [Command.Internal("PixiEditor.Stylus.StylusDown")]
    public void StylusDown(StylusButtonEventArgs e)
    {
        e.Handled = true;

        if (e.StylusButton.Guid == StylusPointProperties.TipButton.Id && e.Inverted)
        {
            PreviousTool = Owner.ToolsSubViewModel.ActiveTool;
            Owner.ToolsSubViewModel.SetActiveTool<EraserToolViewModel>(true);
            ToolSetByStylus = true;
        }
    }

    [Command.Internal("PixiEditor.Stylus.StylusUp")]
    public void StylusUp(StylusButtonEventArgs e)
    {
        e.Handled = true;

        if (ToolSetByStylus && e.StylusButton.Guid == StylusPointProperties.TipButton.Id && e.Inverted)
        {
            Owner.ToolsSubViewModel.SetActiveTool(PreviousTool, false);
        }
    }
}
