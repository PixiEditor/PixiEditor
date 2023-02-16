using System.Windows.Media;

namespace PixiEditor.Models.Commands.Search;

internal class ColorSearchResult : SearchResult
{
    private readonly DrawingImage icon;
    private readonly DrawingApi.Core.ColorsImpl.Color color;
    private string text;
    private bool requiresDocument;
    private bool isPalettePaste;
    private readonly Action<DrawingApi.Core.ColorsImpl.Color> target;

    public override string Text => text;

    public override string Description => $"{color} rgba({color.R}, {color.G}, {color.B}, {color.A})";

    //public override bool CanExecute => !requiresDocument || (requiresDocument && ViewModelMain.Current.BitmapManager.ActiveDocument != null);
    public override bool CanExecute => !isPalettePaste || ViewModelMain.Current.DocumentManagerSubViewModel.ActiveDocument != null;

    public override ImageSource Icon => icon;

    public override void Execute() => target(color);

    private ColorSearchResult(DrawingApi.Core.ColorsImpl.Color color, Action<DrawingApi.Core.ColorsImpl.Color> target)
    {
        this.color = color;
        icon = GetIcon(color);
        this.target = target;
    }

    public ColorSearchResult(DrawingApi.Core.ColorsImpl.Color color) : this(color, x => ViewModelMain.Current.ColorsSubViewModel.PrimaryColor = x)
    {
        text = $"Set color to {color}";
    }

    public static ColorSearchResult PastePalette(DrawingApi.Core.ColorsImpl.Color color, string searchTerm = null)
    {
        //var result = new ColorSearchResult(color, x => ViewModelMain.Current.BitmapManager.ActiveDocument.Palette.Add(x))
        var result = new ColorSearchResult(color, x => ViewModelMain.Current.DocumentManagerSubViewModel.ActiveDocument!.Palette.Add(x))
        {
            SearchTerm = searchTerm,
            isPalettePaste = true
        };
        result.text = $"Add color {color} to palette";
        result.requiresDocument = true;

        return result;
    }

    public static DrawingImage GetIcon(DrawingApi.Core.ColorsImpl.Color color)
    {
        var drawing = new GeometryDrawing() { Brush = new SolidColorBrush(color.ToOpaqueMediaColor()), Pen = new(Brushes.White, 1) };
        var geometry = new EllipseGeometry(new(5, 5), 5, 5) { };
        drawing.Geometry = geometry;
        return new DrawingImage(drawing);
    }
}
