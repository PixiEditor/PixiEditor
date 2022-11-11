using ChunkyImageLib.DataHolders;
using ChunkyImageLib.Shaders;
using ComputeSharp;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Numerics;

namespace ChunkyImageLib.Operations;
internal class ReplaceColorOperation : IDrawOperation
{
    private readonly Color oldColor;
    private readonly Color newColor;

    private readonly ColorBounds oldColorBounds;
    private readonly HlslColorBounds oldColorBoundsHlsl;
    private readonly ulong newColorBits;

    public bool IgnoreEmptyChunks => true;

    public ReplaceColorOperation(Color oldColor, Color newColor)
    {
        this.oldColor = oldColor;
        this.newColor = newColor;
        oldColorBounds = new ColorBounds(oldColor);
        oldColorBoundsHlsl = new HlslColorBounds(new Float4(oldColor.R, oldColor.G, oldColor.B, oldColor.A));
        newColorBits = newColor.ToULong();
    }

    public void DrawOnChunk(Chunk chunk, VecI chunkPos)
    {
        ReplaceColor(oldColorBoundsHlsl, newColor, chunk);
    }

    private static void ReplaceColor(HlslColorBounds oldColorBounds, Color newColor, Chunk chunk)
    {
        Span<UInt2> span = chunk.Surface.DrawingSurface.PeekPixels().GetPixelSpan<UInt2>();
        using var texture = GraphicsDevice.GetDefault()
            .AllocateReadWriteTexture2D<UInt2>(chunk.PixelSize.X, chunk.PixelSize.Y);

        texture.CopyFrom(span);

        UInt2 packedColor = ShaderUtils.PackPixel(newColor);
        
        GraphicsDevice.GetDefault().For(texture.Width, texture.Height, 1,  8, 8, 1,
            new ReplaceColorShader(
                texture,
                oldColorBounds,
                packedColor));
        texture.CopyTo(span);
    }

    public HashSet<VecI> FindAffectedChunks(VecI imageSize)
    {
        return OperationHelper.FindChunksTouchingRectangle(new RectI(VecI.Zero, imageSize), ChunkyImage.FullChunkSize);
    }

    public IDrawOperation AsMirrored(int? verAxisX, int? horAxisY)
    {
        return new ReplaceColorOperation(oldColor, newColor);
    }

    public void Dispose() { }
}
