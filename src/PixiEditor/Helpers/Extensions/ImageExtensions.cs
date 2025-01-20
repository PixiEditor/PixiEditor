using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace PixiEditor.Helpers.Extensions;

public static class ImageExtensions
{
    public static Bitmap? ToBitmap(this IImage? image, PixelSize dimensions)
    {
        if (image is null)
        {
            return null;
        }
        
        RenderTargetBitmap renderTarget = new RenderTargetBitmap(dimensions);
        var context = renderTarget.CreateDrawingContext();
        
        Rect rect = new Rect(0, 0, dimensions.Width, dimensions.Height);
        image.Draw(context, rect, rect);
        
        context.Dispose();
        
        return renderTarget;
    }
}
