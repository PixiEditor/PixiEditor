using System.Globalization;
using Avalonia;
using PixiEditor.Models.Handlers;
using PixiEditor.UI.Common.Converters;
using PixiEditor.UI.Common.Fonts;
using PixiEditor.ViewModels.Tools.Tools;

namespace PixiEditor.Helpers.Converters;

internal class LayerTypeIconConverter : MarkupConverter
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not ILayerHandler layer)
        {
            return AvaloniaProperty.UnsetValue;
        }

        Type? quickEditTool = layer.QuickEditTool;

        if (quickEditTool == typeof(TextToolViewModel))
        {
            return PixiPerfectIcons.TextRound;
        }

        if (quickEditTool == typeof(VectorPathToolViewModel))
        {
            return PixiPerfectIcons.VectorPen;
        }

        if (quickEditTool == typeof(VectorLineToolViewModel))
        {
            return PixiPerfectIcons.Line;
        }

        if (quickEditTool == typeof(VectorEllipseToolViewModel))
        {
            return PixiPerfectIcons.Circle;
        }

        if (quickEditTool == typeof(VectorRectangleToolViewModel))
        {
            return PixiPerfectIcons.Square;
        }

        if (quickEditTool == typeof(PenToolViewModel))
        {
            return PixiPerfectIcons.Pen;
        }

        return layer.Icon;
    }
}
