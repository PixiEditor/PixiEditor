using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using Drawie.Numerics;
using PixiEditor.SVG.Elements;
using PixiEditor.SVG.Features;
using PixiEditor.SVG.Units;

namespace PixiEditor.SVG;

public class SvgParser
{
    private static Dictionary<string, Type> wellKnownElements = new()
    {
        { "ellipse", typeof(SvgEllipse) },
        { "rect", typeof(SvgRectangle) },
        { "circle", typeof(SvgCircle) },
        { "line", typeof(SvgLine) },
        { "path", typeof(SvgPath) },
        { "g", typeof(SvgGroup) },
        { "mask", typeof(SvgMask) },
        { "image", typeof(SvgImage) },
        { "svg", typeof(SvgDocument) },
        { "text", typeof(SvgText) },
        { "linearGradient", typeof(SvgLinearGradient) },
        { "radialGradient", typeof(SvgRadialGradient) },
        { "stop", typeof(SvgStop) },
        { "defs", typeof(SvgDefs) },
        { "clipPath", typeof(SvgClipPath) }
    };

    public string Source { get; set; }

    public SvgParser(string xml)
    {
        Source = xml;
    }

    public SvgDocument? Parse()
    {
        XDocument document = XDocument.Parse(Source);
        using var reader = document.CreateReader();

        XmlNodeType node = reader.MoveToContent();
        if (node != XmlNodeType.Element || reader.LocalName != "svg")
        {
            return null;
        }

        SvgDocument root = (SvgDocument)ParseElement(reader, new SvgDefs())!;

        RectD bounds = ParseBounds(reader); // this takes into account viewBox, width, height, x, y

        root.ViewBox.Unit = new SvgRectUnit(bounds);

        using var defsReader = document.CreateReader();
        root.Defs = ParseDefs(defsReader);

        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                SvgElement? element = ParseElement(reader, root.Defs);
                if (element != null)
                {
                    root.Children.Add(element);

                    if (element is IElementContainer container && element.TagName != "defs")
                    {
                        ParseChildren(reader, container, root.Defs, element.TagName);
                    }
                }
            }
        }

        return root;
    }

    private SvgDefs ParseDefs(XmlReader reader)
    {
        XmlNodeType node = reader.MoveToContent();
        if (node != XmlNodeType.Element)
        {
            return null;
        }

        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "defs")
            {
                break;
            }
        }

        SvgDefs defs = new();
        ParseChildren(reader, defs, new SvgDefs(), "defs");
        return defs;
    }

    private void ParseChildren(XmlReader reader, IElementContainer container, SvgDefs defs, string tagName)
    {
        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                SvgElement? element = ParseElement(reader, defs);
                if (element != null)
                {
                    container.Children.Add(element);

                    if (element is IElementContainer childContainer)
                    {
                        ParseChildren(reader, childContainer, defs, element.TagName);
                    }
                }
            }
            else if (reader.NodeType == XmlNodeType.EndElement && reader.LocalName == tagName)
            {
                break;
            }
        }
    }

    private SvgElement? ParseElement(XmlReader reader, SvgDefs defs)
    {
        if (wellKnownElements.TryGetValue(reader.LocalName, out Type elementType))
        {
            SvgElement element = (SvgElement)Activator.CreateInstance(elementType);
            if (reader.MoveToFirstAttribute())
            {
                element.ParseData(reader, defs);
            }

            return element;
        }

        return null;
    }

    private RectD ParseBounds(XmlReader reader)
    {
        string viewBox = reader.GetAttribute("viewBox");
        string width = reader.GetAttribute("width");
        string height = reader.GetAttribute("height");
        string x = reader.GetAttribute("x");
        string y = reader.GetAttribute("y");

        if (viewBox == null && width == null && height == null && x == null && y == null)
        {
            return new RectD(0, 0, 0, 0);
        }

        double finalX = 0;
        double finalY = 0;
        double finalWidth = 0;
        double finalHeight = 0;

        if (viewBox != null)
        {
            string[] parts = viewBox.Split(' ').Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
            if (parts.Length == 4)
            {
                finalX = double.Parse(parts[0], CultureInfo.InvariantCulture);
                finalY = double.Parse(parts[1], CultureInfo.InvariantCulture);
                finalWidth = double.Parse(parts[2], CultureInfo.InvariantCulture);
                finalHeight = double.Parse(parts[3], CultureInfo.InvariantCulture);
            }
        }

        if (x != null)
        {
            if (double.TryParse(x, CultureInfo.InvariantCulture, out double xValue))
            {
                finalX = xValue;
            }
        }

        if (y != null)
        {
            if (double.TryParse(y, CultureInfo.InvariantCulture, out double yValue))
            {
                finalY = yValue;
            }
        }

        if (width != null)
        {
            if (double.TryParse(width, CultureInfo.InvariantCulture, out double widthValue))
            {
                finalWidth = widthValue;
            }
        }

        if (height != null)
        {
            if (double.TryParse(height, CultureInfo.InvariantCulture, out double heightValue))
            {
                finalHeight = heightValue;
            }
        }


        return new RectD(finalX, finalY, finalWidth, finalHeight);
    }
}
