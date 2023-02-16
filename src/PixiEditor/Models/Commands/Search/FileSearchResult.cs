using System.IO;
using System.Windows.Media;
using PixiEditor.Helpers.Converters;

namespace PixiEditor.Models.Commands.Search;

internal class FileSearchResult : SearchResult
{
    private readonly DrawingImage icon;
    private readonly bool asReferenceLayer;

    public string FilePath { get; }

    public override string Text => asReferenceLayer ? $"As reference: ...\\{Path.GetFileName(FilePath)}" : $"...\\{Path.GetFileName(FilePath)}";

    public override string Description => FilePath;

    public override bool CanExecute => !asReferenceLayer ||
                CommandController.Current.Commands["PixiEditor.Clipboard.PasteReferenceLayerFromPath"].Methods.CanExecute(FilePath);

    public override ImageSource Icon => icon;

    public FileSearchResult(string path, bool asReferenceLayer = false)
    {
        FilePath = path;
        var drawing = new GeometryDrawing() { Brush = FileExtensionToColorConverter.GetBrush(FilePath) };
        var geometry = new RectangleGeometry(new(0, 0, 10, 10), 3, 3) { };
        drawing.Geometry = geometry;
        icon = new DrawingImage(drawing);
        this.asReferenceLayer = asReferenceLayer;
    }

    public override void Execute()
    {
        if (!asReferenceLayer)
        {
            CommandController.Current.Commands["PixiEditor.File.OpenRecent"].Methods.Execute(FilePath);
        }
        else
        {
            var command = CommandController.Current.Commands["PixiEditor.Clipboard.PasteReferenceLayerFromPath"];
            if (command.Methods.CanExecute(FilePath))
            {
                CommandController.Current.Commands["PixiEditor.Clipboard.PasteReferenceLayerFromPath"].Methods
                    .Execute(FilePath);
            }
        }
    }
}
