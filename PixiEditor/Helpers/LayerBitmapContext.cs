using PixiEditor.Models.Layers;
using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PixiEditor.Helpers
{
    class LayerBitmapContext : IDisposable
    {
        public static Color Premultiply(Color c)
        {
            float fraction = c.A / 255f;
            return Color.FromArgb(c.A, (byte)Math.Round(c.R * fraction), (byte)Math.Round(c.G * fraction), (byte)Math.Round(c.B * fraction));
        }

        private Layer layer;
        private BitmapContext ctx;
        public LayerBitmapContext(Layer layer)
        {
            this.layer = layer;
            ctx = layer.LayerBitmap.GetBitmapContext();
        }

        public void Dispose() => ctx.Dispose();

        public bool IsPixelMatching(int x, int y, Color premult)
        {
            int realX = x - layer.OffsetX;
            int realY = y - layer.OffsetY;

            if (realX < 0 || realY < 0 || realX >= ctx.Width || realY >= ctx.Height)
                return premult.A == 0;

            unsafe
            {
                int pos = (realY * ctx.Width + realX) * 4;
                byte* pixels = (byte*)ctx.Pixels;
                return pixels[pos] == premult.B && pixels[pos + 1] == premult.G && pixels[pos + 2] == premult.R && pixels[pos + 3] == premult.A;
            }
        }
    }
}
