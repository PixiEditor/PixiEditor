using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using ChunkyImageLib.DataHolders;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;

namespace ChunkyImageLib.Operations;

internal class PixelsOperation : IMirroredDrawOperation
{
    public bool IgnoreEmptyChunks => false;
    public bool NeedsDrawInSrgb => paint?.Paintable is ISrgbPaintable;

    private readonly VecF[] pixels;
    private readonly Color color;
    private readonly BlendMode blendMode;
    private readonly Paint paint;

    public PixelsOperation(IEnumerable<VecI> pixels, Color color, BlendMode blendMode)
    {
        this.pixels = pixels.Select(pixel => new VecF(pixel.X, pixel.Y)).ToArray();
        this.color = color;
        this.blendMode = blendMode;
        paint = new Paint() { BlendMode = blendMode };
    }

    public void DrawOnChunk(Chunk targetChunk, VecI chunkPos)
    {
        // a hacky way to make the lines look slightly better on non full res chunks
        paint.Color = new Color(color.R, color.G, color.B, (byte)(color.A * targetChunk.Resolution.Multiplier()));

        DrawingSurface surf = targetChunk.Surface.DrawingSurface;
        surf.Canvas.Save();
        surf.Canvas.Scale((float)targetChunk.Resolution.Multiplier());
        surf.Canvas.Translate(-chunkPos * ChunkyImage.FullChunkSize);
        //surf.Canvas.DrawPoints(PointMode.Points, pixels, paint);
        // Drawing points with GPU chunks doesn't work well, that's why we draw rects instead
        foreach (var pixel in pixels)
        {
            surf.Canvas.DrawRect(new RectD((VecD)pixel, new VecD(1)), paint);
        }
        surf.Canvas.Restore();
    }

    public AffectedArea FindAffectedArea(VecI imageSize)
    {
        HashSet<VecI> affectedChunks = new HashSet<VecI>();
        RectI? affectedArea = null;
        foreach (var pixel in pixels)
        {
            affectedChunks.Add(OperationHelper.GetChunkPos(pixel, ChunkyImage.FullChunkSize));
            if (affectedArea is null)
                affectedArea = new RectI(pixel, VecI.One);
            else
                affectedArea = affectedArea.Value.Union(new RectI(pixel, VecI.One));
        }

        return new AffectedArea(affectedChunks, affectedArea);
    }

    public IDrawOperation AsMirrored(double? verAxisX, double? horAxisY)
    {
        var arr = pixels.Select(pixel => new VecI(
            verAxisX is not null ? (int)Math.Round(2 * (double)verAxisX - (int)pixel.X - 1) : (int)pixel.X,
            horAxisY is not null ? (int)Math.Round(2 * (double)horAxisY - (int)pixel.Y - 1) : (int)pixel.Y
        ));
        return new PixelsOperation(arr, color, blendMode);
    }

    public void Dispose()
    {
        paint.Dispose();
    }
}
