using System;
using PixiEditor.DrawingApi.Core.Bridge;
using PixiEditor.DrawingApi.Core.ColorsImpl;

namespace PixiEditor.DrawingApi.Core.Surface
{
    /// <summary>
    ///     Class used to define surface paint, which is a collection of paint operations.
    /// </summary>
    public class Paint : IDisposable
    {
        public Color Color { get; set; }
        public BlendMode BlendMode { get; set; } = BlendMode.Src;
        public FilterQuality FilterQuality { get; set; } = FilterQuality.None;
        public bool IsAntiAliased { get; set; } = false;

        public void Dispose()
        {
            DrawingBackendApi.Current.PaintOperations.Dispose(this);
        }
    }
}
