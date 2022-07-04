using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace PixiEditor.Models.Tools.Brushes;

public abstract class Brush
{
    public abstract void Draw(Layer layer, int toolSize, Coordinates coordinates, SKPaint paint);
}