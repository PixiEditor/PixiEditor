using ChunkyImageLib.DataHolders;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Utils;
using Drawie.Numerics;

namespace ChunkyImageLib.Operations;

internal class RectangleOperation : IMirroredDrawOperation
{
    public ShapeData Data { get; }

    public bool IgnoreEmptyChunks => false;
    public bool NeedsDrawInSrgb => Data.Stroke is ISrgbPaintable || Data.FillPaintable is ISrgbPaintable;

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
        double radiusInPx = Data.CornerRadius * Math.Abs(maxRadiusInPx);

        if (Data.AntiAliasing)
        {
            DrawAntiAliased(surf, rect, radiusInPx);
        }
        else
        {
            DrawPixelPerfect(surf, rect, innerRect, radiusInPx);
        }

        surf.Canvas.RestoreToCount(initial);
    }

    private void DrawPixelPerfect(DrawingSurface surf, RectD rect, RectD innerRect, double radius)
    {
        VecD vecInnerRadius = new VecD(Math.Max(0, radius - Data.StrokeWidth));
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
                surf.Canvas.ClipRoundRect(innerRect, vecInnerRadius, ClipOperation.Intersect);
            }

            surf.Canvas.DrawPaintable(Data.FillPaintable, Data.BlendMode, rect);
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
            surf.Canvas.ClipRoundRect(innerRect, vecInnerRadius, ClipOperation.Difference);
        }

        surf.Canvas.DrawPaintable(Data.Stroke, Data.BlendMode, rect);
    }

    private void DrawAntiAliased(DrawingSurface surf, RectD rect, double radius)
    {
        // shrink radius too so corners match inner curve
        // Draw fill first
        if (Data.FillPaintable != null)
        {
            Data.FillPaintable.Bounds = rect;
        }

        if (Data.Stroke != null)
        {
            Data.Stroke.Bounds = rect;
        }

        if (Data.FillPaintable.AnythingVisible)
        {
            int saved = surf.Canvas.Save();

            paint.StrokeWidth = 0;
            paint.SetPaintable(Data.FillPaintable);
            paint.Style = PaintStyle.Fill;
            RectD fillRect = rect;
            double innerRadius = Math.Max(0, radius - Data.StrokeWidth);
            bool hasStroke = Data is { StrokeWidth: > 0, Stroke.AnythingVisible: true };
            if (hasStroke)
            {
                paint.IsAntiAliased = false;
                fillRect = rect.Inflate(-Data.StrokeWidth + 0.5);
                surf.Canvas.ClipRoundRect(fillRect, new VecD(innerRadius), ClipOperation.Intersect);
            }

            if (radius == 0)
            {
                surf.Canvas.DrawRect((float)fillRect.Left, (float)fillRect.Top,
                    (float)fillRect.Width, (float)fillRect.Height, paint);
            }
            else
            {
                if (hasStroke)
                {
                    surf.Canvas.DrawPaintable(Data.FillPaintable, Data.BlendMode);
                }
                else
                {
                    surf.Canvas.DrawRoundRect((float)fillRect.Left, (float)fillRect.Top,
                        (float)fillRect.Width, (float)fillRect.Height,
                        (float)innerRadius, (float)innerRadius, paint);
                }
            }

            surf.Canvas.RestoreToCount(saved);
        }

        bool hasFill = Data.FillPaintable.AnythingVisible;

        // Draw stroke fully inside
        if (Data.StrokeWidth > 0)
        {
            surf.Canvas.Save();

            paint.StrokeWidth = Data.StrokeWidth;
            paint.SetPaintable(Data.Stroke);
            paint.Style = PaintStyle.Stroke;
            paint.IsAntiAliased = Data.AntiAliasing;

            // shrink rect so stroke is fully inside
            RectD innerRect = rect.Inflate(-Data.StrokeWidth / 2f);

            double innerRadius = Math.Max(0, radius - Data.StrokeWidth / 2f);

            if (radius > 0 && innerRadius <= 0)
            {
                innerRadius = 0.0001;
            }

            if (innerRadius == 0)
            {
                surf.Canvas.DrawRect((float)innerRect.Left, (float)innerRect.Top,
                    (float)innerRect.Width, (float)innerRect.Height, paint);
            }
            else
            {
                surf.Canvas.DrawRoundRect((float)innerRect.Left, (float)innerRect.Top,
                    (float)innerRect.Width, (float)innerRect.Height,
                    (float)innerRadius, (float)innerRadius, paint);
            }

            if(Data.FillPaintable != null)
                Data.FillPaintable.Bounds = null;

            if(Data.Stroke != null)
                Data.Stroke.Bounds = null;

            surf.Canvas.Restore();
        }
    }

    public AffectedArea FindAffectedArea(VecI imageSize)
    {
        if (Math.Abs(Data.Size.X) < 1 || Math.Abs(Data.Size.Y) < 1 ||
            (Data.Stroke is not { AnythingVisible: true } && Data.FillPaintable is not { AnythingVisible: true }))
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

        VecD radiusShrink = new VecD(Data.CornerRadius * Math.Min(Data.Size.X, Data.Size.Y),
            Data.CornerRadius * Math.Min(Data.Size.X, Data.Size.Y));
        VecD innerSize = Data.Size.Abs() - radiusShrink;
        chunks.ExceptWith(
            OperationHelper.FindChunksFullyInsideRectangle(
                Data.Center,
                innerSize - new VecD(Data.StrokeWidth * 2, Data.StrokeWidth * 2),
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
