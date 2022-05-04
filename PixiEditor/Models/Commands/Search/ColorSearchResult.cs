using PixiEditor.Helpers.Extensions;
using PixiEditor.ViewModels;
using SkiaSharp;
using System.Windows.Media;

namespace PixiEditor.Models.Commands.Search
{
    public class ColorSearchResult : SearchResult
    {
        private readonly DrawingImage icon;
        private readonly SKColor color;

        public override string Text => $"Set color to {color}";

        public override string Description => $"{color} rgba({color.Red}, {color.Green}, {color.Blue}, {color.Alpha})";

        public override bool CanExecute => true;

        public override ImageSource Icon => icon;

        public override void Execute() => ViewModelMain.Current.ColorsSubViewModel.PrimaryColor = color;

        public ColorSearchResult(SKColor color)
        {
            this.color = color;
            icon = GetIcon(color);
        }

        public static DrawingImage GetIcon(SKColor color)
        {
            var drawing = new GeometryDrawing() { Brush = new SolidColorBrush(color.ToColor()), Pen = new(Brushes.White, 1) };
            var geometry = new EllipseGeometry(new(5, 5), 5, 5) { };
            drawing.Geometry = geometry;
            return new DrawingImage(drawing);
        }
    }
}
