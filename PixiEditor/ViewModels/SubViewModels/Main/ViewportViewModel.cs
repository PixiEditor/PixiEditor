using PixiEditor.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace PixiEditor.ViewModels.SubViewModels.Main
{
    public class ViewportViewModel : SubViewModel<ViewModelMain>
    {
        public RelayCommand ZoomCommand { get; set; }

        private double _zoomPercentage = 100;

        public double ZoomPercentage
        {
            get { return _zoomPercentage; }
            set
            {
                _zoomPercentage = value;
                RaisePropertyChanged(nameof(ZoomPercentage));
            }
        }

        private Point _viewPortPosition;

        public Point ViewportPosition
        {
            get => _viewPortPosition;
            set
            {
                _viewPortPosition = value;
                RaisePropertyChanged(nameof(ViewportPosition));
            }
        }

        private bool _recenterZoombox;
        public bool RecenterZoombox
        {
            get => _recenterZoombox;
            set
            {
                _recenterZoombox = value;
                RaisePropertyChanged(nameof(RecenterZoombox));
            }
        }


        public ViewportViewModel(ViewModelMain owner) : base(owner)
        {
            ZoomCommand = new RelayCommand(ZoomViewport);

        }

        public void CenterViewport()
        {
            RecenterZoombox = !RecenterZoombox; //It's a trick to trigger change in UserControl
        }

        private void ZoomViewport(object parameter)
        {
            double zoom = (int)parameter;
            ZoomPercentage = zoom;
            ZoomPercentage = 100;
        }
    }
}
