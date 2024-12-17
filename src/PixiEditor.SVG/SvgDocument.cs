using System.Xml;
using System.Xml.Linq;
using Drawie.Numerics;
using PixiEditor.SVG.Features;
using PixiEditor.SVG.Units;

namespace PixiEditor.SVG;

public class SvgDocument : SvgElement, IElementContainer, ITransformable, IFillable, IStrokable
{
    public string RootNamespace { get; set; } = "http://www.w3.org/2000/svg";
    public string Version { get; set; } = "1.1";
    
    public SvgProperty<SvgRectUnit> ViewBox { get; } = new("viewBox");
    public SvgProperty<SvgColorUnit> Fill { get; } = new("fill");
    public SvgProperty<SvgColorUnit> Stroke { get; } = new("stroke");
    public SvgProperty<SvgNumericUnit> StrokeWidth { get; } = new("stroke-width");
    public SvgProperty<SvgTransformUnit> Transform { get; } = new("transform");
    public List<SvgElement> Children { get; } = new();

    public SvgDocument() : base("svg")
    {
        
    }
    
    public SvgDocument(RectD viewBox) : base("svg")
    {
        ViewBox.Unit = new SvgRectUnit(viewBox);
    }

    public override void ParseData(XmlReader reader)
    {
        List<SvgProperty> properties = new()
        {
            Fill,
            Stroke,
            StrokeWidth,
            Transform
        };
        
        ParseAttributes(properties, reader);
    }

    public string ToXml()
    {
        XDocument document = new XDocument();
        document.Declaration = new XDeclaration("1.0", "UTF-8", "yes");
        XNamespace ns = RootNamespace;
        document.Add(new XElement(
            ns + "svg",
            new XAttribute("version", Version))
        );

        Dictionary<string, string> usedNamespaces = new();

        GatherRequiredNamespaces(usedNamespaces, Children);

        foreach (var usedNamespace in usedNamespaces)
        {
            document.Root.Add(new XAttribute($"xmlns:{usedNamespace.Key}", usedNamespace.Value));
        }

        AppendProperties(document.Root);

        foreach (SvgElement child in Children)
        {
            document.Root.Add(child.ToXml(ns));
        }

        return document.ToString();
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

    private void AppendProperties(XElement? root)
    {
        if(ViewBox.Unit != null)
        {
            root.Add(new XAttribute("viewBox", ViewBox.Unit.Value.ToXml()));
        }
        
        if (Fill.Unit != null)
        {
            root.Add(new XAttribute("fill", Fill.Unit.Value.ToXml()));
        }

        if (Stroke.Unit != null)
        {
            root.Add(new XAttribute("stroke", Stroke.Unit.Value.ToXml()));
        }

        if (StrokeWidth.Unit != null)
        {
            root.Add(new XAttribute("stroke-width", StrokeWidth.Unit.Value.ToXml()));
        }
        
        if (Transform.Unit != null)
        {
            root.Add(new XAttribute("transform", Transform.Unit.Value.ToXml()));
        }
    }
}
