using System.Windows;
using PixiEditor.Helpers;

namespace PixiEditor.ViewModels.SubViewModels.Main
{
    public class ViewportViewModel : SubViewModel<ViewModelMain>
    {
        public RelayCommand ZoomCommand { get; set; }

        private double zoomPercentage = 100;

        public double ZoomPercentage
        {
            get => zoomPercentage;
            set
            {
                zoomPercentage = value;
                RaisePropertyChanged(nameof(ZoomPercentage));
            }
        }

        private Point viewPortPosition;

        public Point ViewportPosition
        {
            get => viewPortPosition;
            set
            {
                viewPortPosition = value;
                RaisePropertyChanged(nameof(ViewportPosition));
            }
        }

        private bool recenterZoombox;

        public bool RecenterZoombox
        {
            get => recenterZoombox;
            set
            {
                recenterZoombox = value;
                RaisePropertyChanged(nameof(RecenterZoombox));
            }
        }

        public ViewportViewModel(ViewModelMain owner)
            : base(owner)
        {
            ZoomCommand = new RelayCommand(ZoomViewport);
        }

        public void CenterViewport()
        {
            RecenterZoombox = false; // It's a trick to trigger change in UserControl
            RecenterZoombox = true;
            ViewportPosition = default;
            ZoomPercentage = default;
        }

        private void ZoomViewport(object parameter)
        {
            double zoom = (int)parameter;
            ZoomPercentage = zoom;
            ZoomPercentage = 100;
        }
    }
}