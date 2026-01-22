using System.Xml;
using System.Xml.Linq;
using Drawie.Numerics;
using PixiEditor.SVG.Elements;
using PixiEditor.SVG.Enums;
using PixiEditor.SVG.Features;
using PixiEditor.SVG.Units;

namespace PixiEditor.SVG;

public class SvgDocument : SvgElement, IElementContainer, ITransformable, IFillable, IStrokable, IOpacity, IDefsStorage, ITextData
{
    public string RootNamespace { get; set; } = "http://www.w3.org/2000/svg";
    public string Version { get; set; } = "1.1";

    public SvgProperty<SvgRectUnit> ViewBox { get; } = new("viewBox");
    public SvgProperty<SvgNumericUnit> Width { get; } = new("width");
    public SvgProperty<SvgNumericUnit> Height { get; } = new("height");
    public SvgProperty<SvgNumericUnit> X { get; } = new("x");
    public SvgProperty<SvgNumericUnit> Y { get; } = new("y");
    public SvgProperty<SvgPaintServerUnit> Fill { get; } = new("fill");
    public SvgProperty<SvgPaintServerUnit> Stroke { get; } = new("stroke");
    public SvgProperty<SvgNumericUnit> StrokeOpacity { get; } = new("stroke-opacity");
    public SvgProperty<SvgNumericUnit> StrokeWidth { get; } = new("stroke-width");
    public SvgProperty<SvgPreserveAspectRatioUnit> PreserveAspectRatio { get; } = new("preserveAspectRatio");
    public SvgProperty<SvgEnumUnit<SvgStrokeLineCap>> StrokeLineCap { get; } = new("stroke-linecap");
    public SvgProperty<SvgEnumUnit<SvgStrokeLineJoin>> StrokeLineJoin { get; } = new("stroke-linejoin");
    public SvgProperty<SvgTransformUnit> Transform { get; } = new("transform");
    public SvgProperty<SvgNumericUnit> Opacity { get; } = new("opacity");
    public SvgProperty<SvgNumericUnit> FillOpacity { get; } = new("fill-opacity");
    public SvgProperty<SvgStringUnit> FontFamily { get; } = new("font-family");
    public SvgProperty<SvgNumericUnit> FontSize { get; } = new("font-size");
    public SvgProperty<SvgEnumUnit<SvgFontWeight>> FontWeight { get; } = new("font-weight");
    public SvgProperty<SvgEnumUnit<SvgFontStyle>> FontStyle { get; } = new("font-style");

    public SvgDefs Defs { get; set; } = new();
    public List<SvgElement> Children { get; } = new();

    public SvgDocument() : base("svg")
    {
    }

    public SvgDocument(RectD viewBox) : base("svg")
    {
        ViewBox.Unit = new SvgRectUnit(viewBox);
    }

    public override void ParseAttributes(XmlReader reader, SvgDefs defs)
    {
        List<SvgProperty> properties = new()
        {
            Fill,
            FillOpacity,
            Stroke,
            StrokeOpacity,
            StrokeWidth,
            Transform,
            ViewBox,
            StrokeLineCap,
            StrokeLineJoin,
            Opacity,
            PreserveAspectRatio,
            Width,
            Height,
            X,
            Y,
            FontFamily,
            FontSize,
            FontWeight,
            FontStyle
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

        DefStorage defs = new(this);

        AppendProperties(document.Root, defs);


        foreach (SvgElement child in Children)
        {
            document.Root.Add(child.ToXml(ns, defs));
        }

        if (Defs?.Children.Count > 0)
        {
            document.Root.Add(Defs.ToXml(ns, defs));
            GatherRequiredNamespaces(usedNamespaces, Defs.Children);
        }

        foreach (var usedNamespace in usedNamespaces)
        {
            document.Root.Add(new XAttribute(XNamespace.Xmlns + usedNamespace.Key, usedNamespace.Value));
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

    private void AppendProperties(XElement? root, DefStorage defs)
    {
        if (ViewBox.Unit != null)
        {
            root.Add(new XAttribute("viewBox", ViewBox.Unit.Value.ToXml(defs)));
        }

        if (Fill.Unit != null)
        {
            root.Add(new XAttribute("fill", Fill.Unit.Value.ToXml(defs)));
        }

        if (Stroke.Unit != null)
        {
            root.Add(new XAttribute("stroke", Stroke.Unit.Value.ToXml(defs)));
        }

        if (StrokeWidth.Unit != null)
        {
            root.Add(new XAttribute("stroke-width", StrokeWidth.Unit.Value.ToXml(defs)));
        }

        if (Transform.Unit != null)
        {
            root.Add(new XAttribute("transform", Transform.Unit.Value.ToXml(defs)));
        }

        if (StrokeLineCap.Unit != null)
        {
            root.Add(new XAttribute("stroke-linecap", StrokeLineCap.Unit.Value.ToXml(defs)));
        }

        if (StrokeLineJoin.Unit != null)
        {
            root.Add(new XAttribute("stroke-linejoin", StrokeLineJoin.Unit.Value.ToXml(defs)));
        }
    }
}
