using PixiEditor.Helpers;
using SkiaSharp;
using System;

namespace PixiEditor.ViewModels.SubViewModels.Main
{
    public class ColorsViewModel : SubViewModel<ViewModelMain>
    {
        public RelayCommand SwapColorsCommand { get; set; }

        public RelayCommand SelectColorCommand { get; set; }

        public RelayCommand RemoveSwatchCommand { get; set; }

        public RelayCommand<int> SelectPaletteColorCommand { get; set; }

        private SKColor primaryColor = SKColors.Black;

        public SKColor PrimaryColor // Primary color, hooked with left mouse button
        {
            get => primaryColor;
            set
            {
                if (primaryColor != value)
                {
                    primaryColor = value;
                    Owner.BitmapManager.PrimaryColor = value;
                    RaisePropertyChanged("PrimaryColor");
                }
            }
        }

        private SKColor secondaryColor = SKColors.White;

        public SKColor SecondaryColor
        {
            get => secondaryColor;
            set
            {
                if (secondaryColor != value)
                {
                    secondaryColor = value;
                    RaisePropertyChanged("SecondaryColor");
                }
            }
        }

        public ColorsViewModel(ViewModelMain owner)
            : base(owner)
        {
            SelectColorCommand = new RelayCommand(SelectColor);
            RemoveSwatchCommand = new RelayCommand(RemoveSwatch);
            SwapColorsCommand = new RelayCommand(SwapColors);
            SelectPaletteColorCommand = new RelayCommand<int>(SelectPaletteColor);
        }

        private void SelectPaletteColor(int number)
        {
            var document = Owner.BitmapManager.ActiveDocument;
            if(document.Palette != null && document.Palette.Count >= number)
            {
                PrimaryColor = document.Palette[number - 1];
            }
        }

        public void SwapColors(object parameter)
        {
            var tmp = PrimaryColor;
            PrimaryColor = SecondaryColor;
            SecondaryColor = tmp;
        }

        public void AddSwatch(SKColor color)
        {
            if (!Owner.BitmapManager.ActiveDocument.Swatches.Contains(color))
            {
                Owner.BitmapManager.ActiveDocument.Swatches.Add(color);
            }
        }

        private void RemoveSwatch(object parameter)
        {
            if (!(parameter is SKColor))
            {
                throw new ArgumentException();
            }

            SKColor color = (SKColor)parameter;
            if (Owner.BitmapManager.ActiveDocument.Swatches.Contains(color))
            {
                Owner.BitmapManager.ActiveDocument.Swatches.Remove(color);
            }
        }

        private void SelectColor(object parameter)
        {
            PrimaryColor = parameter as SKColor? ?? throw new ArgumentException();
        }
    }
}
