using Drawie.Backend.Core.Surfaces.PaintImpl;
using PixiEditor.SVG.Elements;
using PixiEditor.SVG.Features;

namespace PixiEditor.SVG.Units;

public struct SvgFilterUnit : ISvgUnit
{
    public SvgFilterPrimitive? ImageFilter { get; set; } = null;

    public SvgLinkUnit? LinksTo { get; set; } = null;

    public SvgFilterUnit()
    {
    }

    public string ToXml(DefStorage defs)
    {
        if (LinksTo != null)
        {
            return LinksTo.Value.ToXml(defs);
        }

        if (ImageFilter != null)
        {
            return TrySerialize(ImageFilter, defs);
        }

        return string.Empty;
    }

    public void ValuesFromXml(string readerValue, SvgDefs defs)
    {
        var linkUnit = new SvgLinkUnit();
        linkUnit.ValuesFromXml(readerValue, defs);
        LinksTo = linkUnit;
        if (!string.IsNullOrEmpty(LinksTo.Value.ObjectReference))
        {
            if (defs.TryFindElement(LinksTo.Value.ObjectReference, out SvgElement? element) &&
                element is IImageFilter filter)
            {
                ImageFilter = filter.GetImageFilter();
            }
        }
    }

    private string TrySerialize(SvgFilterPrimitive imageFilter, DefStorage defs)
    {
        // TODO: Implement serialization for ImageFilter
        return "";
    }
}
