using System.Windows;
using PixiEditor.Helpers;

namespace PixiEditor.ViewModels.SubViewModels.Main
{
    public class ViewportViewModel : SubViewModel<ViewModelMain>
    {
        public RelayCommand ZoomCommand { get; set; }

        public ViewportViewModel(ViewModelMain owner)
            : base(owner)
        {
            ZoomCommand = new RelayCommand(ZoomViewport);
        }

        private void ZoomViewport(object parameter)
        {
            double zoom = (int)parameter;
            Owner.BitmapManager.ActiveDocument.ZoomPercentage = zoom;
            Owner.BitmapManager.ActiveDocument.ZoomPercentage = 100;
        }
    }
}