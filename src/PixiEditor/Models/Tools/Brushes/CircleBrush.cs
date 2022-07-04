using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools.Tools;
using PixiEditor.Models.Tools.ToolSettings.Settings;
using SkiaSharp;
using System;

namespace PixiEditor.Models.Tools.Brushes;

public class CircleBrush : Brush
{
    public override void Draw(Layer layer, int toolSize, Coordinates coordinates, SKPaint paint)
    {
        int halfSize = (int)Math.Ceiling(toolSize / 2f);
        int modifier = toolSize % 2 != 0 ? 1 : 0;
        Coordinates topLeft = new Coordinates(coordinates.X - halfSize + modifier, coordinates.Y - halfSize + modifier);
        Coordinates bottomRight = new Coordinates(coordinates.X + halfSize - 1, coordinates.Y + halfSize - 1);

        CircleTool.DrawEllipseFromCoordinates(layer, topLeft, bottomRight, paint.Color, paint.Color, 1, true);
    }
}