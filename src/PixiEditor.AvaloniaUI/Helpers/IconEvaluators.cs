using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using PixiEditor.AvaloniaUI.Models.Commands.Attributes.Evaluators;
using PixiEditor.AvaloniaUI.Models.Commands.Commands;
using PixiEditor.AvaloniaUI.Models.Commands.Search;

namespace PixiEditor.Helpers;

internal static class IconEvaluators
{
    /*TODO: Segoe is Windows only*/
    private static readonly FontFamily segeoMdl2 = new FontFamily("Segoe MDL2 Assets");

    [Evaluator.Icon("PixiEditor.FontIcon")]
    public static IImage GetFontIcon(object parameter)
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
