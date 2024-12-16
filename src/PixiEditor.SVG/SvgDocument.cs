using System.Text;
using Drawie.Numerics;
using PixiEditor.SVG.Features;

namespace PixiEditor.SVG;

public class SvgDocument(RectD viewBox) : IElementContainer
{
    public string RootNamespace { get; set; } = "http://www.w3.org/2000/svg";
    public string Version { get; set; } = "1.1";
    public RectD ViewBox { get; set; } = viewBox;
    public List<SvgElement> Children { get; } = new();

    public string ToXml()
    {
        StringBuilder builder = new();
        builder.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
        builder.AppendLine(
            $"<svg xmlns=\"{RootNamespace}\" version=\"{Version}\" viewBox=\"{ViewBox.X} {ViewBox.Y} {ViewBox.Width} {ViewBox.Height}\"");

        Dictionary<string, string> usedNamespaces = new();

        GatherRequiredNamespaces(usedNamespaces, Children);

        foreach (var usedNamespace in usedNamespaces)
        {
            builder.AppendLine(
                $"xmlns:{usedNamespace.Key}=\"{usedNamespace.Value}\"");
        }
        
        builder.AppendLine(">");

        foreach (SvgElement child in Children)
        {
            builder.AppendLine(child.ToXml());
        }

        builder.AppendLine("</svg>");

        return builder.ToString();
    }

    public static SvgDocument Parse(string xml)
    {
        SvgParser parser = new(xml);
        return parser.Parse();
    }

    private void GatherRequiredNamespaces(Dictionary<string, string> usedNamespaces, List<SvgElement> elements)
    {
        foreach (SvgElement child in elements)
        {
            if (child is IElementContainer container)
            {
                GatherRequiredNamespaces(usedNamespaces, container.Children);
            }
            foreach (KeyValuePair<string, string> ns in child.RequiredNamespaces)
            {
                usedNamespaces[ns.Key] = ns.Value;
            }
        }
    }
}
