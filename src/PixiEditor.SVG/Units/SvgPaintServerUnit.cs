using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using PixiEditor.SVG.Elements;
using PixiEditor.SVG.Features;

namespace PixiEditor.SVG.Units;

public struct SvgPaintServerUnit : ISvgUnit
{
    public Paintable Paintable { get; set; }

    public SvgLinkUnit? LinksTo { get; set; } = null;

    public SvgPaintServerUnit(Paintable paintable)
    {
        Paintable = paintable;
    }

    public static SvgPaintServerUnit FromColor(Color color)
    {
        return new SvgPaintServerUnit(new ColorPaintable(color));
    }

    public string ToXml()
    {
        throw new NotImplementedException();
    }

    public void ValuesFromXml(string readerValue, SvgDefs defs)
    {
        var linkUnit = new SvgLinkUnit();
        linkUnit.ValuesFromXml(readerValue, defs);
        LinksTo = linkUnit;
        if (string.IsNullOrEmpty(LinksTo.Value.ObjectReference))
        {
            LinksTo = null;
            SvgColorUnit colorUnit = new SvgColorUnit();
            colorUnit.ValuesFromXml(readerValue, defs);
            Paintable = new ColorPaintable(colorUnit.Color);
        }
        else
        {
            if(defs.TryFindElement(LinksTo.Value.ObjectReference, out SvgElement? element) && element is IPaintServer server)
            {
                Paintable = server.GetPaintable();
            }
        }
    }
}
