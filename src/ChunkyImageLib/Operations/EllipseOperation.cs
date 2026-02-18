using ChunkyImageLib.DataHolders;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;

namespace ChunkyImageLib.Operations;

internal class EllipseOperation : IMirroredDrawOperation
{
    public bool IgnoreEmptyChunks => false;
    public bool NeedsDrawInSrgb => strokePaintable is ISrgbPaintable || fillPaintable is ISrgbPaintable;

    private readonly RectD location;
    private readonly Paintable strokePaintable;
    private readonly Paintable fillPaintable;
    private readonly float strokeWidth;
    private readonly double rotation;
    private readonly Paint paint;
    private bool init = false;
    private VectorPath? outerPath;
    private VectorPath? innerPath;

    private VectorPath? ellipseOutline;
    private VecF[]? ellipse;
    private VecF[]? ellipseFill;
    private RectI? ellipseFillRect;
    private bool antialiased;

    public EllipseOperation(RectD location, Paintable strokePaintable, Paintable fillPaintable, float strokeWidth,
        double rotationRad,
        bool antiAliased, Paint? paint = null)
    {
        this.location = location;
        this.strokePaintable = strokePaintable;
        this.fillPaintable = fillPaintable;
        this.strokeWidth = strokeWidth;
        this.rotation = rotationRad;
        this.paint = paint?.Clone() ?? new Paint();
        antialiased = antiAliased;
    }

    private void Init()
    {
        init = true;
        if (strokeWidth - 1 < 0.01)
        {
            if (Math.Abs(rotation) < 0.001)
            {
                if (strokeWidth == 0)
                {
                    ellipseOutline = EllipseHelper.ConstructEllipseOutline((RectI)location);
                }
                else
                {
                    var ellipseList = EllipseHelper.GenerateEllipseFromRect((RectI)location);

                    ellipse = ellipseList.Select(a => new VecF(a)).ToArray();

                    if (fillPaintable.AnythingVisible || paint.BlendMode != BlendMode.SrcOver)
                    {
                        (var fill, ellipseFillRect) =
                            EllipseHelper.SplitEllipseFillIntoRegions(ellipseList.ToList(), (RectI)location);
                        ellipseFill = fill.Select(a => new VecF(a)).ToArray();
                    }
                }
            }
            else
            {
                ellipseOutline = EllipseHelper.GenerateEllipseVectorFromRect(location);
            }
        }
        else
        {
            outerPath = new VectorPath();
            outerPath.ArcTo(location, 0, 359, true);
            innerPath = new VectorPath();
            innerPath.ArcTo(location.Inflate(-strokeWidth), 0, 359, true);
        }
    }

    public void DrawOnChunk(Chunk targetChunk, VecI chunkPos)
    {
        if (!init)
            Init();
        var surf = targetChunk.Surface.DrawingSurface;
        surf.Canvas.Save();
        surf.Canvas.Scale((float)targetChunk.Resolution.Multiplier());
        surf.Canvas.Translate(-chunkPos * ChunkyImage.FullChunkSize);

        paint.IsAntiAliased = antialiased || targetChunk.Resolution != ChunkResolution.Full;

        if (antialiased)
        {
            DrawAntiAliased(surf);
        }
        else
        {
            DrawAliased(surf);
        }

        surf.Canvas.Restore();
    }

    private void DrawAliased(DrawingSurface surf)
    {
        paint.IsAntiAliased = false;
        if (strokeWidth - 1 < 0.01)
        {
            if (Math.Abs(rotation) < 0.001 && strokeWidth > 0)
            {
                RectD rect = (((RectD?)(ellipseFillRect)) ?? (RectD?)location).Value;
                fillPaintable.Bounds = location;
                if (fillPaintable.AnythingVisible || paint.BlendMode != BlendMode.SrcOver)
                {
                    paint.SetPaintable(fillPaintable);
                    if (ellipseFill is { Length: > 0 })
                    {
                        VecF lastPt = ellipseFill[0];
                        surf.Canvas.DrawRect(lastPt.X, lastPt.Y, 1, 1, paint);
                        for (var index = 1; index < ellipseFill.Length; index++)
                        {
                            var pt = ellipseFill[index];
                            VecD roundedLastPt = new(Math.Floor(lastPt.X), Math.Floor(lastPt.Y));
                            VecD roundedPt = new(Math.Floor(pt.X), Math.Floor(pt.Y));
                            surf.Canvas.DrawLine(roundedLastPt, roundedPt, paint);
                            lastPt = pt;
                        }

                        // DrawPoints don't work well on GPU surfaces
                        //surf.Canvas.DrawPoints(PointMode.Lines, ellipseFill!, paint);
                        surf.Canvas.DrawRect(rect, paint);
                    }
                }

                paint.SetPaintable(strokeWidth <= 0 ? fillPaintable : strokePaintable);
                paint.StrokeWidth = 1f;

                foreach (var pt in ellipse!)
                {
                    surf.Canvas.DrawRect(pt.X, pt.Y, 1, 1, paint);
                }
                // DrawPoints don't work well on GPU surfaces
                //surf.Canvas.DrawPoints(PointMode.Points, ellipse!, paint);

                fillPaintable.Bounds = null;
            }
            else
            {
                surf.Canvas.Save();
                surf.Canvas.RotateRadians((float)rotation, (float)location.Center.X, (float)location.Center.Y);

                if (fillPaintable.AnythingVisible || paint.BlendMode != BlendMode.SrcOver)
                {
                    paint.SetPaintable(fillPaintable);
                    paint.Style = PaintStyle.Fill;
                    surf.Canvas.DrawPath(ellipseOutline!, paint);
                }

                if (strokeWidth > 0)
                {
                    paint.SetPaintable(strokePaintable);
                    paint.Style = PaintStyle.Stroke;
                    paint.StrokeWidth = 1;

                    surf.Canvas.DrawPath(ellipseOutline!, paint);
                }

                surf.Canvas.Restore();
            }
        }
        else
        {
            if (fillPaintable.AnythingVisible || paint.BlendMode != BlendMode.SrcOver)
            {
                surf.Canvas.Save();
                surf.Canvas.RotateRadians((float)rotation, (float)location.Center.X, (float)location.Center.Y);
                surf.Canvas.ClipPath(innerPath!);
                surf.Canvas.DrawPaintable(fillPaintable, paint.BlendMode, location);
                surf.Canvas.Restore();
            }

            surf.Canvas.Save();
            surf.Canvas.RotateRadians((float)rotation, (float)location.Center.X, (float)location.Center.Y);
            surf.Canvas.ClipPath(outerPath!);
            surf.Canvas.ClipPath(innerPath!, ClipOperation.Difference);
            surf.Canvas.DrawPaintable(strokePaintable, paint.BlendMode, location);
            surf.Canvas.Restore();
        }
    }

    private void DrawAntiAliased(DrawingSurface surf)
    {
        surf.Canvas.Save();
        surf.Canvas.RotateRadians((float)rotation, (float)location.Center.X, (float)location.Center.Y);

        paint.IsAntiAliased = false;
        paint.SetPaintable(fillPaintable);
        paint.Style = PaintStyle.Fill;

        RectD fillRect = ((RectD)location).Inflate(-strokeWidth / 2f);

        surf.Canvas.DrawOval(fillRect.Center, fillRect.Size / 2f, paint);

        paint.IsAntiAliased = true;
        paint.SetPaintable(strokeWidth <= 0 ? fillPaintable : strokePaintable);
        paint.Style = PaintStyle.Stroke;
        paint.StrokeWidth = strokeWidth <= 0 ? 1f : strokeWidth;

        RectD strokeRect = ((RectD)location).Inflate((-strokeWidth / 2f));

        surf.Canvas.DrawOval(strokeRect.Center, strokeRect.Size / 2f, paint);

        surf.Canvas.Restore();
    }

    public AffectedArea FindAffectedArea(VecI imageSize)
    {
        ShapeCorners corners = new((RectD)location);
        corners = corners.AsRotated(rotation, (VecD)location.Center);
        RectI bounds = (RectI)corners.AABBBounds.RoundOutwards();

        var chunks = OperationHelper.FindChunksTouchingRectangle(bounds, ChunkyImage.FullChunkSize);
        if (!fillPaintable?.AnythingVisible ?? false)
        {
            chunks.ExceptWith(OperationHelper.FindChunksFullyInsideEllipse
            (location.Center, location.Width / 2.0 - strokeWidth * 2, location.Height / 2.0 - strokeWidth * 2,
                ChunkyImage.FullChunkSize, rotation));
        }

        return new AffectedArea(chunks, bounds);
    }

    public IDrawOperation AsMirrored(double? verAxisX, double? horAxisY)
    {
        RectD newLocation = location;
        if (verAxisX is not null)
            newLocation = newLocation.ReflectX((double)verAxisX).Round();
        if (horAxisY is not null)
            newLocation = newLocation.ReflectY((double)horAxisY).Round();

        Paintable? finalFillPaintable = fillPaintable;
        Paintable? finalStrokePaintable = strokePaintable;
        if (fillPaintable.AbsoluteValues && fillPaintable is IPositionPaintable)
        {
            finalFillPaintable = fillPaintable.Clone();
            ((IPositionPaintable)finalFillPaintable).Position = newLocation.Center;
        }

        if (strokePaintable.AbsoluteValues && strokePaintable is IPositionPaintable)
        {
            finalStrokePaintable = strokePaintable.Clone();
            ((IPositionPaintable)finalStrokePaintable).Position = newLocation.Center;
        }

        return new EllipseOperation(newLocation, finalStrokePaintable, finalFillPaintable, strokeWidth, rotation,
            antialiased, paint);
    }

    public void Dispose()
    {
        paint.Dispose();
        outerPath?.Dispose();
        innerPath?.Dispose();
        ellipseOutline?.Dispose();
    }
}
