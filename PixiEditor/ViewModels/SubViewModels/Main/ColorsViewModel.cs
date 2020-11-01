using PixiEditor.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace PixiEditor.ViewModels.SubViewModels.Main
{
    public class ColorsViewModel : SubViewModel<ViewModelMain>
    {
        public RelayCommand SwapColorsCommand { get; set; }
        public RelayCommand SelectColorCommand { get; set; }
        public RelayCommand RemoveSwatchCommand { get; set; }

        private Color _primaryColor = Colors.Black;
        public Color PrimaryColor //Primary color, hooked with left mouse button
        {
            get => _primaryColor;
            set
            {
                if (_primaryColor != value)
                {
                    _primaryColor = value;
                    Owner.BitmapManager.PrimaryColor = value;
                    RaisePropertyChanged("PrimaryColor");
                }
            }
        }

        private Color _secondaryColor = Colors.White;
        public Color SecondaryColor
        {
            get => _secondaryColor;
            set
            {
                if (_secondaryColor != value)
                {
                    _secondaryColor = value;
                    RaisePropertyChanged("SecondaryColor");
                }
            }
        }

        public ColorsViewModel(ViewModelMain owner) : base(owner)
        {
            SelectColorCommand = new RelayCommand(SelectColor);
            RemoveSwatchCommand = new RelayCommand(RemoveSwatch);
            SwapColorsCommand = new RelayCommand(SwapColors);
        }

        public void SwapColors(object parameter)
        {
            var tmp = PrimaryColor;
            PrimaryColor = SecondaryColor;
            SecondaryColor = tmp;
        }

        public void AddSwatch(Color color)
        {
            if (!Owner.BitmapManager.ActiveDocument.Swatches.Contains(color))
            {
                Owner.BitmapManager.ActiveDocument.Swatches.Add(color);
            }
        }

        private void RemoveSwatch(object parameter)
        {
            if (!(parameter is Color)) throw new ArgumentException();
            Color color = (Color)parameter;
            if (Owner.BitmapManager.ActiveDocument.Swatches.Contains(color))
            {
                Owner.BitmapManager.ActiveDocument.Swatches.Remove(color);
            }
        }

        private void SelectColor(object parameter)
        {
            PrimaryColor = parameter as Color? ?? throw new ArgumentException();
        }

    }
}
