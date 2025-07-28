using System.Diagnostics.Contracts;
using Drawie.Backend.Core.Surfaces.PaintImpl;

namespace PixiEditor.ChangeableDocument.Changeables.Graph;

public sealed class Filter : IDisposable
{
    public Filter(ColorFilter? colorFilter, ImageFilter? imageFilter)
    {
        ColorFilter = colorFilter;
        ImageFilter = imageFilter;
    }
    
    public ColorFilter? ColorFilter { get; }
    
    public ImageFilter? ImageFilter { get; }

    [Pure]
    public Filter Add(ColorFilter? colorFilter, ImageFilter? imageFilter)
    {
        ColorFilter? color = ColorFilter;
        
        if (colorFilter != null)
        {
            color = ColorFilter == null ? colorFilter : ColorFilter.CreateCompose(colorFilter, ColorFilter);
        }

        ImageFilter? image = ImageFilter;
        if (imageFilter != null)
        {
            image = ImageFilter == null ? imageFilter : ImageFilter.CreateCompose(imageFilter, ImageFilter);
        }

        return new Filter(color, image);
    }

    [Pure]
    public Filter Add(ColorFilter? colorFilter) => Add(colorFilter, null);

    [Pure]
    public Filter Add(ImageFilter? imageFilter) => Add(null, imageFilter);

    public void Dispose()
    {
        ColorFilter?.Dispose();
        ImageFilter?.Dispose();
    }
}
