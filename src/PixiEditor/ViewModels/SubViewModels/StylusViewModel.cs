using PixiEditor.Extensions.CommonApi.UserPreferences;
using PixiEditor.Extensions.CommonApi.UserPreferences.Settings.PixiEditor;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.UI.Common.Fonts;
using PixiEditor.ViewModels.Tools;
using PixiEditor.ViewModels.Tools.Tools;

namespace PixiEditor.ViewModels.SubViewModels;

[Command.Group("PixiEditor.Stylus", "STYLUS")]
internal class StylusViewModel : SubViewModel<ViewModelMain>
{
    private bool isPenModeEnabled;
    private bool useTouchGestures;

    public bool ToolSetByStylus { get; set; }

    private ToolViewModel PreviousTool { get; set; }

    public StylusViewModel(ViewModelMain owner)
        : base(owner)
    {
        isPenModeEnabled = PixiEditorSettings.Tools.IsPenModeEnabled.Value;
    }

    //TODO: Fix stylus support
    /*[Command.Internal("PixiEditor.Stylus.StylusOutOfRange")]
    public void StylusOutOfRange(StylusEventArgs e)
    {
        //Owner.BitmapManager.UpdateHighlightIfNecessary(true);
    }*/

    /*[Command.Internal("PixiEditor.Stylus.StylusSystemGesture")]
    public void StylusSystemGesture(StylusSystemGestureEventArgs e)
    {
        if (e.SystemGesture is SystemGesture.Drag or SystemGesture.Tap)
        {
            return;
        }

        e.Handled = true;
    }*/

    /*[Command.Internal("PixiEditor.Stylus.StylusDown")]
    public void StylusDown(StylusButtonEventArgs e)
    {
        e.Handled = true;

        if (e.StylusButton.Guid == StylusPointProperties.TipButton.Id && e.Inverted)
        {
            PreviousTool = Owner.ToolsSubViewModel.ActiveTool;
            Owner.ToolsSubViewModel.SetActiveTool<EraserToolViewModel>(true);
            ToolSetByStylus = true;
        }
    }*/

    /*[Command.Internal("PixiEditor.Stylus.StylusUp")]
    public void StylusUp(StylusButtonEventArgs e)
    {
        e.Handled = true;

        if (ToolSetByStylus && e.StylusButton.Guid == StylusPointProperties.TipButton.Id && e.Inverted)
        {
            Owner.ToolsSubViewModel.SetActiveTool(PreviousTool, false);
        }
    }*/
}
