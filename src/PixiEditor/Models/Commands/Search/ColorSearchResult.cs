using System.Windows.Media;
using PixiEditor.Helpers.Extensions;
using PixiEditor.ViewModels;
using SkiaSharp;

namespace PixiEditor.Models.Commands.Search;

internal class ColorSearchResult : SearchResult
{
    private readonly DrawingImage icon;
    private readonly SKColor color;
    private string text;
    private bool requiresDocument;
    private readonly Action<SKColor> target;

    public override string Text => text;

    public override string Description => $"{color} rgba({color.Red}, {color.Green}, {color.Blue}, {color.Alpha})";

    //public override bool CanExecute => !requiresDocument || (requiresDocument && ViewModelMain.Current.BitmapManager.ActiveDocument != null);
    public override bool CanExecute => false;

    public override ImageSource Icon => icon;

    public override void Execute() => target(color);

    private ColorSearchResult(SKColor color, Action<SKColor> target)
    {
        this.color = color;
        icon = GetIcon(color);
        this.target = target;
    }

    public ColorSearchResult(SKColor color) : this(color, x => ViewModelMain.Current.ColorsSubViewModel.PrimaryColor = x)
    {
        text = $"Set color to {color}";
    }

    public static ColorSearchResult PastePalette(SKColor color, string searchTerm = null)
    {
        //var result = new ColorSearchResult(color, x => ViewModelMain.Current.BitmapManager.ActiveDocument.Palette.Add(x))
        var result = new ColorSearchResult(color, x => { })
        {
            SearchTerm = searchTerm
        };
        result.text = $"Add color {color} to palette";
        result.requiresDocument = true;

        return result;
    }

    public static DrawingImage GetIcon(SKColor color)
    {
        var drawing = new GeometryDrawing() { Brush = new SolidColorBrush(color.ToOpaqueColor()), Pen = new(Brushes.White, 1) };
        var geometry = new EllipseGeometry(new(5, 5), 5, 5) { };
        drawing.Geometry = geometry;
        return new DrawingImage(drawing);
    }
}
