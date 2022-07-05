using System.Windows.Input;
using PixiEditor.Models.Commands.Attributes;

namespace PixiEditor.ViewModels.SubViewModels.Main;

internal class ViewportViewModel : SubViewModel<ViewModelMain>
{
    private bool gridLinesEnabled;

    public bool GridLinesEnabled
    {
        get => gridLinesEnabled;
        set => SetProperty(ref gridLinesEnabled, value);
    }

    public ViewportViewModel(ViewModelMain owner)
        : base(owner)
    {
    }

    [Command.Basic("PixiEditor.View.ToggleGrid", "Toggle gridlines", "Toggle gridlines", Key = Key.OemTilde, Modifiers = ModifierKeys.Control)]
    public void ToggleGridLines()
    {
        GridLinesEnabled = !GridLinesEnabled;
    }

    [Command.Basic("PixiEditor.View.ZoomIn", 1, "Zoom in", "Zoom in", CanExecute = "PixiEditor.HasDocument", Key = Key.OemPlus)]
    [Command.Basic("PixiEditor.View.Zoomout", -1, "Zoom out", "Zoom out", CanExecute = "PixiEditor.HasDocument", Key = Key.OemMinus)]
    public void ZoomViewport(double zoom)
    {
        /*
        if (Owner.BitmapManager.ActiveDocument is not null)
        {
            Owner.BitmapManager.ActiveDocument.ZoomViewportTrigger.Execute(this, zoom);
        }
        */
    }
}
