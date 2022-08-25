using System;
using System.Collections.Generic;
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
        Span<UInt2> span = chunk.Surface.SkiaSurface.PeekPixels().GetPixelSpan<uint2>();
        using var texture = GraphicsDevice.GetDefault()
            .AllocateReadWriteTexture2D<uint2>(chunk.PixelSize.X, chunk.PixelSize.Y);

        texture.CopyFrom(span);
        
        uint convR = (BitConverter.HalfToUInt16Bits((Half)(newColor.Red / 255f)));
        uint convG = (BitConverter.HalfToUInt16Bits((Half)(newColor.Green / 255f)));
        uint convB = (BitConverter.HalfToUInt16Bits((Half)(newColor.Blue / 255f)));
        uint convA = (BitConverter.HalfToUInt16Bits((Half)(newColor.Alpha / 255f)));

        UInt2 newCol = new UInt2(convG << 16 | convR, convB | convA << 16);
        
        GraphicsDevice.GetDefault().For(texture.Width, texture.Height, 
            new ReplaceColorShader(
                texture,
                oldColorBounds,
                newCol));
        texture.CopyTo(span);
        //SKImage processedImage = SKBitmap.
        
        /*int maxThreads = Environment.ProcessorCount;
        VecI imageSize = chunk.PixelSize;
        int rowsPerThread = imageSize.Y / maxThreads;

        using SKPixmap pixmap = chunk.Surface.SkiaSurface.PeekPixels();
        IntPtr pixels = pixmap.GetPixels();

        Half* endOffset = (Half*)(pixels + pixmap.BytesSize);
        for (Half* i = (Half*)pixels; i < endOffset; i += 4)
        {
            if (oldColorBounds.IsWithinBounds(i))
                *(ulong*)i = newColorBits;
        }*/
    }

    private static Float4 ToFloat4(UInt2 pixel)
    {
        return new Float4(
            Hlsl.Float16ToFloat32(pixel.X),
            Hlsl.Float16ToFloat32(pixel.X >> 16),
            Hlsl.Float16ToFloat32(pixel.Y),
            Hlsl.Float16ToFloat32(pixel.Y >> 16));
    }

    private static ulong PackPixel(Float4 pixel)
    {
        return (ulong)pixel.R << 0 | (ulong)pixel.G << 8 | (ulong)pixel.B << 16 | (ulong)pixel.A << 24;
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
