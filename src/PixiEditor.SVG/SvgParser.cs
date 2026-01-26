using System.Globalization;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using Drawie.Numerics;
using PixiEditor.SVG.Elements;
using PixiEditor.SVG.Features;
using PixiEditor.SVG.Units;

namespace PixiEditor.SVG;

public class SvgParser
{
    private static Dictionary<string, Type> wellKnownElements = Assembly.GetExecutingAssembly()
        .GetTypes()
        .Where(t => t.IsSubclassOf(typeof(SvgElement)) && !t.IsAbstract)
        .ToDictionary(t => (Activator.CreateInstance(t) as SvgElement).TagName, t => t);

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

        using var defsReader = document.CreateReader();
        root.Defs = ParseDefs(defsReader);

        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                if (reader.LocalName == "defs")
                {
                    // already parsed defs, skip
                    reader.Skip();
                    if(reader.NodeType != XmlNodeType.Element)
                    {
                        continue;
                    }
                }

                SvgElement? element = ParseElement(reader, root.Defs);
                if (element != null)
                {
                    root.Children.Add(element);

                    if (element is IElementContainer container)
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
            element.ParseElement(reader, defs);

            return element;
        }

        return null;
    }
}
