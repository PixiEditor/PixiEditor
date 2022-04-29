using PixiEditor.Models.Commands.Attributes;
using SkiaSharp;
using System.Windows.Input;

namespace PixiEditor.ViewModels.SubViewModels.Main
{
    public class ColorsViewModel : SubViewModel<ViewModelMain>
    {
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
        }

        [Command.Basic("PixiEditor.Colors.Swap", "Swap colors", "Swap primary and secondary colors", Key = Key.X)]
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

        [Command.Internal("PixiEditor.Colors.RemoveSwatch")]
        public void RemoveSwatch(SKColor color)
        {
            if (Owner.BitmapManager.ActiveDocument.Swatches.Contains(color))
            {
                Owner.BitmapManager.ActiveDocument.Swatches.Remove(color);
            }
        }

        [Command.Internal("PixiEditor.Colors.SelectColor")]
        public void SelectColor(SKColor color)
        {
            PrimaryColor = color;
        }
    }
}
