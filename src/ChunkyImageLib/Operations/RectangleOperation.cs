using ChunkyImageLib.DataHolders;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;

namespace ChunkyImageLib.Operations;

internal class RectangleOperation : IMirroredDrawOperation
{
    public ShapeData Data { get; }

    public bool IgnoreEmptyChunks => false;

    private Paint paint = new();

    public RectangleOperation(ShapeData rect)
    {
        Data = rect;
        paint.StrokeWidth = Data.StrokeWidth;
        paint.IsAntiAliased = Data.AntiAliasing;
        paint.BlendMode = Data.BlendMode;
    }


    public void DrawOnChunk(Chunk targetChunk, VecI chunkPos)
    {
        var surf = targetChunk.Surface.DrawingSurface;

        var rect = RectD.FromCenterAndSize(Data.Center, Data.Size.Abs());
        var innerRect = rect.Inflate(-Data.StrokeWidth);
        if (innerRect.IsZeroOrNegativeArea)
            innerRect = RectD.Empty;

        int initial = surf.Canvas.Save();


        surf.Canvas.Scale((float)targetChunk.Resolution.Multiplier());
        surf.Canvas.Translate(-chunkPos * ChunkyImage.FullChunkSize);
        surf.Canvas.RotateRadians((float)Data.Angle, (float)rect.Center.X, (float)rect.Center.Y);

        double maxRadiusInPx = Math.Min(Data.Size.X, Data.Size.Y) / 2;
        double radiusInPx = Data.CornerRadius * maxRadiusInPx;

        if (Data.AntiAliasing)
        {
            DrawAntiAliased(surf, rect, innerRect, radiusInPx);
        }
        else
        {
            DrawPixelPerfect(surf, rect, innerRect, radiusInPx);
        }

        surf.Canvas.RestoreToCount(initial);
    }

    private void DrawPixelPerfect(DrawingSurface surf, RectD rect, RectD innerRect, double radius)
    {
        // draw fill
        if (Data.FillPaintable.AnythingVisible)
        {
            int saved = surf.Canvas.Save();
            if (radius == 0)
            {
                surf.Canvas.ClipRect(innerRect);
            }
            else
            {
                surf.Canvas.ClipRoundRect(innerRect, new VecD(radius), ClipOperation.Intersect);
            }

            surf.Canvas.DrawPaintable(Data.FillPaintable, Data.BlendMode);
            surf.Canvas.RestoreToCount(saved);
        }

        // draw stroke
        surf.Canvas.Save();
        if (radius == 0)
        {
            surf.Canvas.ClipRect(rect);
            surf.Canvas.ClipRect(innerRect, ClipOperation.Difference);
        }
        else
        {
            VecD vecRadius = new VecD(radius);
            surf.Canvas.ClipRoundRect(rect, vecRadius, ClipOperation.Intersect);
            surf.Canvas.ClipRoundRect(innerRect, vecRadius, ClipOperation.Difference);
        }

        surf.Canvas.DrawPaintable(Data.Stroke, Data.BlendMode);
    }

    private void DrawAntiAliased(DrawingSurface surf, RectD rect, RectD innerRect, double radius)
    {
        surf.Canvas.Save();
        paint.StrokeWidth = Data.StrokeWidth > 0 ? Data.StrokeWidth : 1;
        paint.SetPaintable(Data.StrokeWidth > 0 ? Data.Stroke : Data.FillPaintable);
        paint.Style = PaintStyle.Fill;

        if (radius == 0)
        {
            surf.Canvas.DrawRect((float)rect.Left, (float)rect.Top, (float)rect.Width,
                (float)rect.Height, paint);
        }
        else
        {
            surf.Canvas.DrawRoundRect((float)rect.Left, (float)rect.Top, (float)rect.Width,
                (float)rect.Height, (float)radius, (float)radius, paint);
        }

        // draw fill
        if (Data.FillPaintable.AnythingVisible)
        {
            int saved = surf.Canvas.Save();

            paint.StrokeWidth = 0;
            paint.SetPaintable(Data.FillPaintable);
            paint.Style = PaintStyle.Fill;
            if (radius == 0)
            {
                surf.Canvas.DrawRect((float)innerRect.Left, (float)innerRect.Top, (float)innerRect.Width, (float)innerRect.Height, paint);
            }
            else
            {
                surf.Canvas.DrawRoundRect((float)innerRect.Left, (float)innerRect.Top, (float)innerRect.Width,
                    (float)innerRect.Height, (float)radius, (float)radius, paint);
            }

            surf.Canvas.RestoreToCount(saved);
        }
    }

    public AffectedArea FindAffectedArea(VecI imageSize)
    {
        if (Math.Abs(Data.Size.X) < 1 || Math.Abs(Data.Size.Y) < 1 ||
            (!Data.Stroke.AnythingVisible && !Data.FillPaintable.AnythingVisible))
            return new();

        RectI affRect = (RectI)new ShapeCorners(Data.Center, Data.Size).AsRotated(Data.Angle, Data.Center).AABBBounds
            .RoundOutwards();

        if (Data.FillPaintable.AnythingVisible || Math.Abs(Data.Size.X) == 1 || Math.Abs(Data.Size.Y) == 1)
            return new(
                OperationHelper.FindChunksTouchingRectangle(Data.Center, Data.Size.Abs(), Data.Angle,
                    ChunkPool.FullChunkSize), affRect);

        var chunks =
            OperationHelper.FindChunksTouchingRectangle(Data.Center, Data.Size.Abs(), Data.Angle,
                ChunkPool.FullChunkSize);
        chunks.ExceptWith(
            OperationHelper.FindChunksFullyInsideRectangle(
                Data.Center,
                Data.Size.Abs() - new VecD(Data.StrokeWidth * 2, Data.StrokeWidth * 2),
                Data.Angle,
                ChunkPool.FullChunkSize));
        return new(chunks, affRect);
    }

    public void Dispose()
    {
        paint.Dispose();
    }

    public IDrawOperation AsMirrored(double? verAxisX, double? horAxisY)
    {
        if (verAxisX is not null && horAxisY is not null)
            return new RectangleOperation(Data.AsMirroredAcrossHorAxis((double)horAxisY)
                .AsMirroredAcrossVerAxis((double)verAxisX));
        else if (verAxisX is not null)
            return new RectangleOperation(Data.AsMirroredAcrossVerAxis((double)verAxisX));
        else if (horAxisY is not null)
            return new RectangleOperation(Data.AsMirroredAcrossHorAxis((double)horAxisY));
        return new RectangleOperation(Data);
    }
}
