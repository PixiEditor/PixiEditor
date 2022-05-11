using PixiEditor.Models.Commands.Attributes;
using SkiaSharp;
using System.Windows.Input;
using PixiEditor.Models.Services;

namespace PixiEditor.ViewModels.SubViewModels.Main
{
    public class ColorsViewModel : SubViewModel<ViewModelMain>
    {
        private SKColor primaryColor = SKColors.Black;
        private DocumentProvider _doc;

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

        public ColorsViewModel(ViewModelMain owner, DocumentProvider provider)
            : base(owner)
        {
            _doc = provider;
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
            var swatches = _doc.GetSwatches();
            if (!swatches.Contains(color))
            {
                swatches.Add(color);
            }
        }

        [Command.Internal("PixiEditor.Colors.RemoveSwatch")]
        public void RemoveSwatch(SKColor color)
        {
            var swatches = _doc.GetSwatches();
            if (swatches.Contains(color))
            {
                swatches.Remove(color);
            }
        }

        [Command.Internal("PixiEditor.Colors.SelectColor")]
        public void SelectColor(SKColor color)
        {
            PrimaryColor = color;
        }
    }
}
