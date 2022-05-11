using PixiEditor.Models.Commands.Attributes;
using System.Windows.Input;
using PixiEditor.Models.Services;

namespace PixiEditor.ViewModels.SubViewModels.Main
{
    public class ViewportViewModel : SubViewModel<ViewModelMain>
    {
        private readonly DocumentProvider _doc;
        private bool gridLinesEnabled;

        public bool GridLinesEnabled
        {
            get => gridLinesEnabled;
            set => SetProperty(ref gridLinesEnabled, value);
        }

        public ViewportViewModel(ViewModelMain owner, DocumentProvider provider)
            : base(owner)
        {
            _doc = provider;
        }

        [Command.Basic("PixiEditor.View.ToggleGrid", "Toggle gridlines", "Toggle gridlines", Key = Key.OemTilde,
            Modifiers = ModifierKeys.Control)]
        public void ToggleGridLines()
        {
            GridLinesEnabled = !GridLinesEnabled;
        }

        [Command.Basic("PixiEditor.View.ZoomIn", 1, "Zoom in", "Zoom in", CanExecute = "PixiEditor.HasDocument",
            Key = Key.OemPlus)]
        [Command.Basic("PixiEditor.View.Zoomout", -1, "Zoom out", "Zoom out", CanExecute = "PixiEditor.HasDocument",
            Key = Key.OemMinus)]
        public void ZoomViewport(double zoom)
        {
            _doc.GetDocument().ZoomViewportTrigger.Execute(this, zoom);
        }
    }
}