using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PixiEditor.Models.Commands.Attributes;
using PixiEditor.Models.Commands.Attributes.Evaluators;
using PixiEditor.Models.Commands.Search;
using Command = PixiEditor.Models.Commands.Commands.Command;

namespace PixiEditor.Helpers;

internal static class IconEvaluators
{
    private static readonly FontFamily segeoMdl2 = new FontFamily("Segoe MDL2 Assets");

    [Evaluator.Icon("PixiEditor.FontIcon")]
    public static ImageSource GetFontIcon(object parameter)
    {
        string symbolCode = GetIconName(parameter);

        var textBlock = new TextBlock
        {
            FontFamily = segeoMdl2,
            Foreground = Brushes.White,
            Text = char.ConvertFromUtf32(int.Parse(symbolCode, NumberStyles.HexNumber)),
        };

        var brush = new VisualBrush
        {
            Visual = textBlock,
            Stretch = Stretch.Uniform
        };

        var drawing = new GeometryDrawing
        {
            Brush = brush,
            Geometry = new RectangleGeometry(
                new Rect(0, 0, 32, 32))
        };

        return new DrawingImage(drawing);
    }

    private static string GetIconName(object parameter)
    {
        return parameter switch
        {
            Command command => command.IconPath,
            CommandSearchResult cmdResult => cmdResult.Command.IconPath,
            _ => throw new NotImplementedException($"Parameter typeof {parameter.GetType()} has not been implemented yet.")
        };
    }
}
