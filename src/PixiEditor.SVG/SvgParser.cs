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
        { "svg", typeof(SvgDocument) }
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
        
        SvgDocument root = (SvgDocument)ParseElement(reader)!;

        RectD bounds = ParseBounds(reader); // this takes into account viewBox, width, height, x, y
        
        root.ViewBox.Unit = new SvgRectUnit(bounds);

        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                SvgElement? element = ParseElement(reader);
                if (element != null)
                {
                    root.Children.Add(element);

                    if (element is IElementContainer container)
                    {
                        ParseChildren(reader, container, element.TagName);
                    }
                }
            }
        }

        return root;
    }

    private void ParseChildren(XmlReader reader, IElementContainer container, string tagName)
    {
        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                SvgElement? element = ParseElement(reader);
                if (element != null)
                {
                    container.Children.Add(element);

                    if (element is IElementContainer childContainer)
                    {
                        ParseChildren(reader, childContainer, element.TagName);
                    }
                }
            }
            else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == tagName)
            {
                break;
            }
        }
    }

    private SvgElement? ParseElement(XmlReader reader)
    {
        if (wellKnownElements.TryGetValue(reader.LocalName, out Type elementType))
        {
            SvgElement element = (SvgElement)Activator.CreateInstance(elementType);
            if (reader.MoveToFirstAttribute())
            {
                element.ParseData(reader);
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
                finalX = double.Parse(parts[0]);
                finalY = double.Parse(parts[1]);
                finalWidth = double.Parse(parts[2]);
                finalHeight = double.Parse(parts[3]);
            }
        }

        if (x != null)
        {
            if (double.TryParse(x, out double xValue))
            {
                finalX = xValue;
            }
        }

        if (y != null)
        {
            if (double.TryParse(y, out double yValue))
            {
                finalY = yValue;
            }
        }

        if (width != null)
        {
            if (double.TryParse(width, out double widthValue))
            {
                finalWidth = widthValue;
            }
        }

        if (height != null)
        {
            if (double.TryParse(height, out double heightValue))
            {
                finalHeight = heightValue;
            }
        }


        return new RectD(finalX, finalY, finalWidth, finalHeight);
    }
}
