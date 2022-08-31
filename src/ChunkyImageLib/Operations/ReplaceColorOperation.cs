using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ChunkyImageLib.DataHolders;
using ChunkyImageLib.Shaders;
using ComputeSharp;
using SkiaSharp;

namespace ChunkyImageLib.Operations;
internal class ReplaceColorOperation : IDrawOperation
{
    private readonly SKColor oldColor;
    private readonly SKColor newColor;

    private readonly ColorBounds oldColorBounds;
    private readonly HlslColorBounds oldColorBoundsHlsl;
    private readonly ulong newColorBits;

    public bool IgnoreEmptyChunks => true;

    public ReplaceColorOperation(SKColor oldColor, SKColor newColor)
    {
        this.oldColor = oldColor;
        this.newColor = newColor;
        oldColorBounds = new ColorBounds(oldColor);
        oldColorBoundsHlsl = new HlslColorBounds(new Float4(oldColor.Red, oldColor.Green, oldColor.Blue, oldColor.Alpha));
        newColorBits = newColor.ToULong();
    }

    public void DrawOnChunk(Chunk chunk, VecI chunkPos)
    {
        ReplaceColor(oldColorBoundsHlsl, newColor, chunk);
    }

    private static void ReplaceColor(HlslColorBounds oldColorBounds, SKColor newColor, Chunk chunk)
    {
        Span<UInt2> span = chunk.Surface.DrawingSurface.PeekPixels().GetPixelSpan<uint2>();
        using var texture = GraphicsDevice.GetDefault()
            .AllocateReadWriteTexture2D<uint2>(chunk.PixelSize.X, chunk.PixelSize.Y);

        texture.CopyFrom(span);

        UInt2 packedColor = ShaderUtils.PackPixel(newColor);
        
        GraphicsDevice.GetDefault().For(texture.Width, texture.Height, 
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
