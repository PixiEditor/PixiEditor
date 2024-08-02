using PixiEditor.DrawingApi.Core.Surfaces.ImageData;
using PixiEditor.Numerics;

namespace PixiEditor.DrawingApi.Core.Surfaces;

public class Texture 
{
    public VecI Size { get; }
    
    public DrawingSurface GpuSurface { get; }
    
    public Texture(VecI size)
    {
        Size = size;
        
        GpuSurface = DrawingSurface.Create(new ImageInfo(Size.X, Size.Y, ColorType.RgbaF16, AlphaType.Premul, ColorSpace.CreateSrgb())
        {
            GpuBacked = true
        });
    }
}
