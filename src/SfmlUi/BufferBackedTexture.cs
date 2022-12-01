using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.ImageData;
using SFML.Graphics;

namespace SfmlUi;
internal class BufferBackedTexture : IDisposable
{
    public Texture Texture { get; private set; }
    public DrawingSurface Surface { get; private set; }
    private nint buffer;
    public VecI Size { get; }

    private List<RectI> dirtyRects = new();

    public BufferBackedTexture(VecI size)
    {
        using SFML.Graphics.Image tempImage = new((uint)size.X, (uint)size.Y);
        Texture = new Texture(tempImage);
        buffer = Marshal.AllocHGlobal(size.X * size.Y * 4);
        Surface = DrawingSurface.Create(new ImageInfo(size.X, size.Y, ColorType.Rgba8888), buffer);
        this.Size = size;
    }

    public void AddDirtyRect(RectI dirtyRect)
    {
        dirtyRects.Add(dirtyRect);
    }

    public unsafe void UpdateTextureFromBuffer()
    {
        RectI textureRect = new RectI(VecI.Zero, Size);
        foreach (var rect in dirtyRects)
        {
            var fixedRect = rect.Intersect(textureRect);
            if (fixedRect.IsZeroOrNegativeArea)
                continue;

            byte[] pixels = new byte[fixedRect.Width * fixedRect.Height * 4];

            int w = fixedRect.Width * 4;
            int h = fixedRect.Height;

            for (int j = 0; j < h; j++)
            {
                for (int i = 0; i < w; i++)
                {
                    int globalX = i + fixedRect.X * 4;
                    int globalY = j + fixedRect.Y;

                    int posInBuffer = globalY * Size.X * 4 + globalX;
                    int posInPixels = j * w + i;

                    pixels[posInPixels] = *((byte*)buffer + posInBuffer);
                }
            }

            Texture.Update(pixels, (uint)fixedRect.Width, (uint)fixedRect.Height, (uint)fixedRect.X, (uint)fixedRect.Y);
        }
        dirtyRects.Clear();
    }

    public void Dispose()
    {
        Texture.Dispose();
        Surface.Dispose();
        Marshal.FreeHGlobal(buffer);
    }
}
