using System;
using System.Collections.Generic;
using System.Linq;
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
        ReplaceColor(oldColorBoundsHlsl, oldColor, newColor, chunk);
    }

    private static void ReplaceColor(HlslColorBounds oldColorBounds, SKColor oldColor, SKColor newColor, Chunk chunk)
    {
        var span = chunk.Surface.SkiaSurface.PeekPixels().GetPixelSpan<Rgba64>();
        using var texture = GraphicsDevice.GetDefault()
            .AllocateReadWriteTexture2D<Rgba64, float4>(span, chunk.PixelSize.X, chunk.PixelSize.Y);
        
        GraphicsDevice.GetDefault().For(texture.Width, texture.Height, 
            new ReplaceColorShader(
                texture,
                oldColorBounds,
                new Float3(newColor.Red / 255f, newColor.Green / 255f, newColor.Blue / 255f)));
        Rgba64[] pixels = new Rgba64[texture.Width * texture.Height];
        texture.CopyTo(pixels);
        ApplyPixelsToChunk(chunk, pixels);
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

    private static unsafe void ApplyPixelsToChunk(Chunk chunk, Rgba64[] pixels)
    {
        using var drawPixmap = chunk.Surface.SkiaSurface.PeekPixels();
        Half* drawArray = (Half*)drawPixmap.GetPixels();

        for (int i = 0; i < pixels.Length; i++)
        {
            ulong pixel = pixels[i].PackedValue;
            Half* drawPixel = drawArray + i * 4;
            *(ulong*)drawPixel = pixel;
        }
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
