using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Rendering.SceneGraph;
using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using PixiEditor.DrawingApi.Core.Numerics;

namespace PixiEditor.AvaloniaUI.Views.Renderers;

public class ChunkControl : Control
{
    public static readonly StyledProperty<Chunk> ChunkProperty = AvaloniaProperty.Register<ChunkControl, Chunk>(
        nameof(Chunk));

    public static readonly StyledProperty<VecI> ChunkPositionProperty = AvaloniaProperty.Register<ChunkControl, VecI>(
        nameof(ChunkPosition));

    public VecI ChunkPosition
    {
        get => GetValue(ChunkPositionProperty);
        set => SetValue(ChunkPositionProperty, value);
    }
    public Chunk Chunk
    {
        get => GetValue(ChunkProperty);
        set => SetValue(ChunkProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        return base.MeasureOverride(availableSize);
    }

    public override void Render(DrawingContext context)
    {
        context.PushTransform(Matrix.CreateScale(new Vector(Chunk.Resolution.InvertedMultiplier(), Chunk.Resolution.InvertedMultiplier())));
        Color color = Color.FromArgb(255, (byte)Random.Shared.Next(0, 255), (byte)Random.Shared.Next(0, 255), (byte)Random.Shared.Next(0, 255));
        context.DrawRectangle(new SolidColorBrush(color), new Pen(Brushes.Black, 0), new Rect(0, 0, Chunk.PixelSize.X, Chunk.PixelSize.Y));
    }
}


public class RenderChunkOperation : ICustomDrawOperation
{
    public Rect Bounds { get; }

    public void Render(ImmediateDrawingContext context)
    {
        throw new NotImplementedException();
    }

    public bool Equals(ICustomDrawOperation? other)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public bool HitTest(Point p)
    {
        throw new NotImplementedException();
    }
}
