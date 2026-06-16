using System.Xml;
using PixiEditor.SVG.Features;

namespace PixiEditor.SVG.Elements;

public class SvgDefs() : SvgElement("defs"), IElementContainer
{
    public List<SvgElement> Children { get; } = new();

    public bool TryFindElement(string id, out SvgElement element)
    {
        return TryFindElement(Children, id, out element);
    }

    public override void ParseAttributes(XmlReader reader, SvgDefs defs)
    {

    }

    private bool TryFindElement(List<SvgElement> root, string id, out SvgElement? element)
    {
        if (root == null || root.Count == 0)
        {
            element = null;
            return false;
        }

        foreach (SvgElement child in root)
        {
            if (child.Id.Unit?.Value == id)
            {
                element = child;
                return true;
            }

            if (child is IElementContainer container && container.Children.Count != 0)
            {
                if (TryFindElement(container.Children, id, out element))
                {
                    return true;
                }
            }
        }

        element = null;
        return false;
    }
}
