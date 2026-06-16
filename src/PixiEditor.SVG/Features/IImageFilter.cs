using PixiEditor.SVG.Elements;

namespace PixiEditor.SVG.Features;

public interface IImageFilter
{
    public SvgFilterPrimitive? GetImageFilter();
}
