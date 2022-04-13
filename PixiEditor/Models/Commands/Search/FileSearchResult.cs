using PixiEditor.Helpers.Converters;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace PixiEditor.Models.Commands.Search
{
    public class FileSearchResult : SearchResult
    {
        private readonly DrawingImage icon;

        public string FilePath { get; }

        public override string Text => $"...\\{Path.GetFileName(FilePath)}";

        public override string Description => FilePath;

        public override bool CanExecute => true;

        public override ImageSource Icon => icon;

        public FileSearchResult(string path)
        {
            FilePath = path;
            icon = new();
            var drawing = new GeometryDrawing() { Brush = FileExtensionToColorConverter.GetBrush(FilePath) };
            var geometry = new RectangleGeometry(new(0, 0, 10, 10), 3, 3) { };
            drawing.Geometry = geometry;
            icon = new DrawingImage(drawing);
        }

        public override void Execute()
        {
            CommandController.Current.Commands["PixiEditor.File.Open"].Methods.Execute(FilePath);
        }
    }
}
