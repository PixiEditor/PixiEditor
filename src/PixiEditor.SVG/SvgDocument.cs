using System.Xml;
using System.Xml.Linq;
using Drawie.Numerics;
using PixiEditor.SVG.Elements;
using PixiEditor.SVG.Enums;
using PixiEditor.SVG.Features;
using PixiEditor.SVG.Units;

namespace PixiEditor.SVG;

public class SvgDocument : SvgElement, IElementContainer, ITransformable, IFillable, IStrokable, IOpacity, IDefsStorage
{
    public string RootNamespace { get; set; } = "http://www.w3.org/2000/svg";
    public string Version { get; set; } = "1.1";

    public SvgProperty<SvgRectUnit> ViewBox { get; } = new("viewBox");
    public SvgProperty<SvgPaintServerUnit> Fill { get; } = new("fill");
    public SvgProperty<SvgPaintServerUnit> Stroke { get; } = new("stroke");
    public SvgProperty<SvgNumericUnit> StrokeWidth { get; } = new("stroke-width");

    public SvgProperty<SvgEnumUnit<SvgStrokeLineCap>> StrokeLineCap { get; } = new("stroke-linecap");

    public SvgProperty<SvgEnumUnit<SvgStrokeLineJoin>> StrokeLineJoin { get; } = new("stroke-linejoin");
    public SvgProperty<SvgTransformUnit> Transform { get; } = new("transform");
    public SvgProperty<SvgNumericUnit> Opacity { get; } = new("opacity");
    public SvgProperty<SvgNumericUnit> FillOpacity { get; } = new("fill-opacity");

    public SvgDefs Defs { get; set; } = new();
    public List<SvgElement> Children { get; } = new();

    public SvgDocument() : base("svg")
    {
    }

    public SvgDocument(RectD viewBox) : base("svg")
    {
        ViewBox.Unit = new SvgRectUnit(viewBox);
    }

    public override void ParseData(XmlReader reader, SvgDefs defs)
    {
        List<SvgProperty> properties = new()
        {
            Fill,
            FillOpacity,
            Stroke,
            StrokeWidth,
            Transform,
            ViewBox,
            StrokeLineCap,
            StrokeLineJoin,
            Opacity,
        };

        ParseAttributes(properties, reader, defs); // TODO: merge with Defs?
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
            document.Root.Add(new XAttribute(XNamespace.Xmlns + usedNamespace.Key, usedNamespace.Value));
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
        if (ViewBox.Unit != null)
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

        if (StrokeLineCap.Unit != null)
        {
            root.Add(new XAttribute("stroke-linecap", StrokeLineCap.Unit.Value.ToXml()));
        }

        if (StrokeLineJoin.Unit != null)
        {
            root.Add(new XAttribute("stroke-linejoin", StrokeLineJoin.Unit.Value.ToXml()));
        }
    }
}
