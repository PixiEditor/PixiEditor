using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Styling;
using PixiEditor.Views;
using PixiEditor.Helpers.Converters;
using PixiEditor.Models.Commands.CommandContext;
using PixiEditor.Models.Commands.Commands;
using PixiEditor.UI.Common.Localization;

namespace PixiEditor.Models.Commands.Search;

internal class FileSearchResult : SearchResult
{
    private readonly DrawingImage icon;
    private readonly FileSearchTarget target;

    public string FilePath { get; }

    public override string Text
    {
        get
        {
            var shortenedPath = GetShortenedPath(FilePath);

            return target switch
            {
                FileSearchTarget.OpenDocument => shortenedPath,
                FileSearchTarget.ReferenceLayer => new LocalizedString("OPEN_PATH_AS_REFERENCE_LAYER", shortenedPath),
                FileSearchTarget.NestedDocument => new LocalizedString("OPEN_PATH_AS_NESTED_DOCUMENT", shortenedPath),
                _ => throw new IndexOutOfRangeException()
            };
        }
    }

    public override AvaloniaObject Description => GetDescription();

    public override bool CanExecute => GetCommandFromTarget(target).Methods.CanExecute(FilePath);

    public override IImage Icon => icon;

    public FileSearchResult(string path, FileSearchTarget target)
    {
        FilePath = path;
        var drawing = new GeometryDrawing() { Brush = FileExtensionToColorConverter.GetBrush(FilePath) };
        var geometry = new RectangleGeometry(new Rect(0, 0, 10, 10)) { RadiusX = 2, RadiusY = 2 };
        drawing.Geometry = geometry;
        icon = new DrawingImage(drawing);
        this.target = target;
    }

    public override void Execute()
    {
        var command = GetCommandFromTarget(target);
        var context = new CommandExecutionContext(FilePath, new SearchSourceInfo());
        
        command.Execute(context, true);
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

    private static Command GetCommandFromTarget(FileSearchTarget target) => target switch
    {
        FileSearchTarget.OpenDocument => CommandController.Current.Commands["PixiEditor.File.OpenFromPath"],
        FileSearchTarget.ReferenceLayer => CommandController.Current.Commands[
            "PixiEditor.Clipboard.PasteReferenceLayerFromPath"],
        FileSearchTarget.NestedDocument => CommandController.Current.Commands["PixiEditor.File.PlaceElementFromPath"],
        _ => throw new IndexOutOfRangeException()
    };

    private static string GetShortenedPath(string path)
    {
        var separatorIndex = path.IndexOfAny(['\\', '/']);
        var character = separatorIndex == -1 ? Path.DirectorySeparatorChar : path[separatorIndex];

        return $"...{character}{Path.GetFileName(path)}";
    }
}
