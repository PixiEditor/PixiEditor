using ChunkyImageLib.DataHolders;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Surfaces.Vector;
using Drawie.Numerics;

namespace ChunkyImageLib.Operations;
internal class EllipseOperation : IMirroredDrawOperation
{
    public bool IgnoreEmptyChunks => false;

    private readonly RectI location;
    private readonly Color strokeColor;
    private readonly Color fillColor;
    private readonly int strokeWidth;
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

    public EllipseOperation(RectI location, Color strokeColor, Color fillColor, int strokeWidth, double rotationRad,
        bool antiAliased, Paint? paint = null)
    {
        this.location = location;
        this.strokeColor = strokeColor;
        this.fillColor = fillColor;
        this.strokeWidth = strokeWidth;
        this.rotation = rotationRad;
        this.paint = paint?.Clone() ?? new Paint();
        antialiased = antiAliased;
    }

    private void Init()
    {
        init = true;
        if (strokeWidth == 1)
        {
            if (Math.Abs(rotation) < 0.001)
            {
                var ellipseList = EllipseHelper.GenerateEllipseFromRect(location);

                ellipse = ellipseList.Select(a => new VecF(a)).ToArray();

                if (fillColor.A > 0 || paint.BlendMode != BlendMode.SrcOver)
                {
                    (var fill, ellipseFillRect) = EllipseHelper.SplitEllipseFillIntoRegions(ellipseList.ToList(), location);
                    ellipseFill = fill.Select(a => new VecF(a)).ToArray();
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
        if (strokeWidth == 1)
        {
            if (Math.Abs(rotation) < 0.001)
            {
                if (fillColor.A > 0 || paint.BlendMode != BlendMode.SrcOver)
                {
                    paint.Color = fillColor;
                    surf.Canvas.DrawPoints(PointMode.Lines, ellipseFill!, paint);
                    surf.Canvas.DrawRect((RectD)ellipseFillRect!.Value, paint);
                }
                
                paint.Color = strokeColor;
                paint.StrokeWidth = 1f;
                surf.Canvas.DrawPoints(PointMode.Points, ellipse!, paint);
            }
            else
            {
                surf.Canvas.Save();
                surf.Canvas.RotateRadians((float)rotation, (float)location.Center.X, (float)location.Center.Y);
                
                if (fillColor.A > 0 || paint.BlendMode != BlendMode.SrcOver)
                {
                    paint.Color = fillColor;
                    paint.Style = PaintStyle.Fill;
                    surf.Canvas.DrawPath(ellipseOutline!, paint);
                }
                
                paint.Color = strokeColor;
                paint.Style = PaintStyle.Stroke;
                paint.StrokeWidth = 1f;
                
                surf.Canvas.DrawPath(ellipseOutline!, paint);

                surf.Canvas.Restore();
            }
        }
        else
        {
            if (fillColor.A > 0 || paint.BlendMode != BlendMode.SrcOver)
            {
                surf.Canvas.Save();
                surf.Canvas.RotateRadians((float)rotation, (float)location.Center.X, (float)location.Center.Y);
                surf.Canvas.ClipPath(innerPath!);
                surf.Canvas.DrawColor(fillColor, paint.BlendMode);
                surf.Canvas.Restore();
            }
            surf.Canvas.Save();
            surf.Canvas.RotateRadians((float)rotation, (float)location.Center.X, (float)location.Center.Y);
            surf.Canvas.ClipPath(outerPath!);
            surf.Canvas.ClipPath(innerPath!, ClipOperation.Difference);
            surf.Canvas.DrawColor(strokeColor, paint.BlendMode);
            surf.Canvas.Restore();
        }
    }

    private void DrawAntiAliased(DrawingSurface surf)
    {
        surf.Canvas.Save();
        surf.Canvas.RotateRadians((float)rotation, (float)location.Center.X, (float)location.Center.Y);
        
        paint.IsAntiAliased = false;
        paint.Color = fillColor;
        paint.Style = PaintStyle.Fill;
        
        RectD fillRect = ((RectD)location).Inflate(-strokeWidth / 2f);
        
        surf.Canvas.DrawOval(fillRect.Center, fillRect.Size / 2f, paint);
        
        paint.IsAntiAliased = true;
        paint.Color = strokeColor;
        paint.Style = PaintStyle.Stroke;
        paint.StrokeWidth = strokeWidth;
        
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
        if (fillColor.A == 0)
        {
             chunks.ExceptWith(OperationHelper.FindChunksFullyInsideEllipse
                (location.Center, location.Width / 2.0 - strokeWidth * 2, location.Height / 2.0 - strokeWidth * 2, ChunkyImage.FullChunkSize, rotation));
        }
        
        return new AffectedArea(chunks, bounds);
    }

    public IDrawOperation AsMirrored(double? verAxisX, double? horAxisY)
    {
        RectI newLocation = location;
        if (verAxisX is not null)
            newLocation = (RectI)newLocation.ReflectX((double)verAxisX).Round();
        if (horAxisY is not null)
            newLocation = (RectI)newLocation.ReflectY((double)horAxisY).Round();
        return new EllipseOperation(newLocation, strokeColor, fillColor, strokeWidth, rotation, antialiased, paint);
    }

    public void Dispose()
    {
        paint.Dispose();
        outerPath?.Dispose();
        innerPath?.Dispose();
    }
}
