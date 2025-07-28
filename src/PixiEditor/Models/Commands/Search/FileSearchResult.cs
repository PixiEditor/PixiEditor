using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Styling;
using PixiEditor.Views;
using PixiEditor.Helpers.Converters;

namespace PixiEditor.Models.Commands.Search;

internal class FileSearchResult : SearchResult
{
    private readonly DrawingImage icon;
    private readonly bool asReferenceLayer;

    public string FilePath { get; }

    public override string Text => asReferenceLayer ? $"As reference: ...\\{Path.GetFileName(FilePath)}" : $"...\\{Path.GetFileName(FilePath)}";

    public override AvaloniaObject Description => GetDescription();

    public override bool CanExecute => !asReferenceLayer ||
                CommandController.Current.Commands["PixiEditor.Clipboard.PasteReferenceLayerFromPath"].Methods.CanExecute(FilePath);

    public override IImage Icon => icon;

    public FileSearchResult(string path, bool asReferenceLayer = false)
    {
        FilePath = path;
        var drawing = new GeometryDrawing() { Brush = FileExtensionToColorConverter.GetBrush(FilePath) };
        var geometry = new RectangleGeometry(new Rect(0, 0, 10, 10)) { RadiusX = 2, RadiusY = 2 };
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

    private TextBlock GetDescription()
    {
        var path = FilePath;
        
        var text = new TextBlock { Inlines = new InlineCollection(), FontSize = 14, TextTrimming = TextTrimming.CharacterEllipsis };

        string[] split = path.Split(Path.DirectorySeparatorChar);
        bool hasEllipsis = false;

        for (var i = 0; i < split.Length; i++)
        {
            if (i > 3 && i < split.Length - 3)
            {
                if (!hasEllipsis)
                {
                    text.Inlines.Add("…"); // Horizontal Ellipsis
                    text.Inlines.Add(GetSeparator());
                    hasEllipsis = true;
                    
                }

                continue;
            }

            string part = split[i];
            text.Inlines.Add(part.Replace(":", string.Empty));
            
            if (i != split.Length - 1)
            {
                text.Inlines.Add(GetSeparator());
            }
        }

        return text;

        Run GetSeparator()
        {
            var separator = new Run(" › ") { FontWeight = FontWeight.Light }; // Single Right-Pointing Angle Quotation Mark

            return separator;
        }
    }
}
