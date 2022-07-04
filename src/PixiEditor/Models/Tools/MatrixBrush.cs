using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools.Brushes;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace PixiEditor.Models.Tools;

public abstract class MatrixBrush : Brush
{
    public abstract int[,] BrushMatrix { get; }

    public SKPoint[] BakedMatrix => _cachedBakedMatrix;


    private SKPoint[] _cachedBakedMatrix;
    private SKPoint[] _cachedGetAtPointBakedMatrix;

    public MatrixBrush()
    {
        InitMatrix();
    }

    public override void Draw(Layer layer, int toolSize, Coordinates coordinates, SKPaint paint)
    {
        layer.LayerBitmap.SkiaSurface.Canvas.DrawPoints(SKPointMode.Points, GetAtPoint(coordinates, layer.OffsetX, layer.OffsetY), paint);
    }

    //We can easily handle .pixi to brush parsing

    /// <summary>
    ///     Creates an SKPoint[] array from BrushMatrix. All values grater than 0 will be placed in final array.
    /// </summary>
    /// <returns>Points array, ready to be drawn on Skia canvas.</returns>
    public virtual SKPoint[] BakeMatrix()
    {
        List<SKPoint> result = new List<SKPoint>();

        int brushHeight = BrushMatrix.GetLength(0);
        int brushWidth = BrushMatrix.GetLength(1);

        int centerX = (int)Math.Floor(brushWidth / 2f);
        int centerY = (int)Math.Floor(brushHeight / 2f);

        for (int i = 0; i < brushHeight; i++)
        {
            for (int j = 0; j < brushWidth; j++)
            {
                if (BrushMatrix[i, j] > 0)
                {
                    result.Add(new SKPoint(centerX - j, centerY - i));
                }
            }
        }

        return result.ToArray();
    }

    /// <summary>
    ///     Calculates BrushMatrix for given point.
    /// </summary>
    /// <param name="point">Point to calculate BrushMatrix for.</param>
    /// <returns>SKPoints for given coordinate.</returns>
    public SKPoint[] GetAtPoint(Coordinates point, int offsetX, int offsetY)
    {
        if (_cachedGetAtPointBakedMatrix == null)
        {
            InitMatrix();
        }

        for (int i = 0; i < _cachedGetAtPointBakedMatrix.Length; i++)
        {
            _cachedGetAtPointBakedMatrix[i] = new SKPoint(
                _cachedBakedMatrix[i].X + point.X - offsetX,
                _cachedBakedMatrix[i].Y + point.Y - offsetY);

        }

        return _cachedGetAtPointBakedMatrix;
    }

    private void InitMatrix()
    {
        if (_cachedBakedMatrix == null)
        {
            _cachedBakedMatrix = BakeMatrix();
            _cachedGetAtPointBakedMatrix = new SKPoint[_cachedBakedMatrix.Length];
            Array.Copy(_cachedBakedMatrix, _cachedGetAtPointBakedMatrix, BakedMatrix.Length);
        }
    }

}