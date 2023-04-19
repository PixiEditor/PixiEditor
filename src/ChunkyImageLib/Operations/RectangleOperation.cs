using ChunkyImageLib.DataHolders;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface;

namespace ChunkyImageLib.Operations;

internal class RectangleOperation : IMirroredDrawOperation
{
    public RectangleOperation(ShapeData rect)
    {
        Data = rect;
    }

    public ShapeData Data { get; }

    public bool IgnoreEmptyChunks => false;

    public void DrawOnChunk(Chunk chunk, VecI chunkPos)
    {
        var skiaSurf = chunk.Surface.DrawingSurface;

        var surf = chunk.Surface.DrawingSurface;

        var rect = RectD.FromCenterAndSize(Data.Center, Data.Size.Abs());
        var innerRect = rect.Inflate(-Data.StrokeWidth);
        if (innerRect.IsZeroOrNegativeArea)
            innerRect = RectD.Empty;

        surf.Canvas.Save();
        surf.Canvas.Scale((float)chunk.Resolution.Multiplier());
        surf.Canvas.Translate(-chunkPos * ChunkyImage.FullChunkSize);
        skiaSurf.Canvas.RotateRadians((float)Data.Angle, (float)rect.Center.X, (float)rect.Center.Y);

        // draw fill
        if (Data.FillColor.A > 0)
        {
            skiaSurf.Canvas.Save();
            skiaSurf.Canvas.ClipRect(innerRect);
            skiaSurf.Canvas.DrawColor(Data.FillColor, Data.BlendMode);
            skiaSurf.Canvas.Restore();
        }

        // draw stroke
        skiaSurf.Canvas.Save();
        skiaSurf.Canvas.ClipRect(rect);
        skiaSurf.Canvas.ClipRect(innerRect, ClipOperation.Difference);
        skiaSurf.Canvas.DrawColor(Data.StrokeColor, Data.BlendMode);
        skiaSurf.Canvas.Restore();

        surf.Canvas.Restore();
    }

    public AffectedArea FindAffectedArea(VecI imageSize)
    {
        if (Math.Abs(Data.Size.X) < 1 || Math.Abs(Data.Size.Y) < 1 || (Data.StrokeColor.A == 0 && Data.FillColor.A == 0))
            return new();

        RectI affRect = (RectI)new ShapeCorners(Data.Center, Data.Size).AsRotated(Data.Angle, Data.Center).AABBBounds.RoundOutwards();

        if (Data.FillColor.A != 0 || Math.Abs(Data.Size.X) == 1 || Math.Abs(Data.Size.Y) == 1)
            return new (OperationHelper.FindChunksTouchingRectangle(Data.Center, Data.Size.Abs(), Data.Angle, ChunkPool.FullChunkSize), affRect);

        var chunks = OperationHelper.FindChunksTouchingRectangle(Data.Center, Data.Size.Abs(), Data.Angle, ChunkPool.FullChunkSize);
        chunks.ExceptWith(
            OperationHelper.FindChunksFullyInsideRectangle(
                Data.Center,
                Data.Size.Abs() - new VecD(Data.StrokeWidth * 2, Data.StrokeWidth * 2),
                Data.Angle,
                ChunkPool.FullChunkSize));
        return new (chunks, affRect);
    }

    public void Dispose() { }

    public IDrawOperation AsMirrored(double? verAxisX, double? horAxisY)
    {
        if (verAxisX is not null && horAxisY is not null)
            return new RectangleOperation(Data.AsMirroredAcrossHorAxis((double)horAxisY).AsMirroredAcrossVerAxis((double)verAxisX));
        else if (verAxisX is not null)
            return new RectangleOperation(Data.AsMirroredAcrossVerAxis((double)verAxisX));
        else if (horAxisY is not null)
            return new RectangleOperation(Data.AsMirroredAcrossHorAxis((double)horAxisY));
        return new RectangleOperation(Data);
    }
}
