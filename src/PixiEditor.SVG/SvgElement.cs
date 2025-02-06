using System.Text;
using System.Xml;
using System.Xml.Linq;
using PixiEditor.SVG.Exceptions;
using PixiEditor.SVG.Features;
using PixiEditor.SVG.Units;

namespace PixiEditor.SVG;

public class SvgElement(string tagName)
{
    public SvgProperty<SvgStringUnit> Id { get; } = new("id");
    public Dictionary<string, string> RequiredNamespaces { get; } = new();
    public string TagName { get; } = tagName;

    public SvgProperty<SvgStyleUnit> Style { get; } = new("style");

    public XElement ToXml(XNamespace nameSpace)
    {
        XElement element = new XElement(nameSpace + TagName);

        foreach (var property in GetType().GetProperties())
        {
            if (property.PropertyType.IsAssignableTo(typeof(SvgProperty)))
            {
                SvgProperty prop = (SvgProperty)property.GetValue(this);
                if (prop?.Unit != null)
                {
                    if (string.IsNullOrEmpty(prop.SvgName))
                    {
                        element.Value = prop.Unit.ToXml();
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(prop.NamespaceName))
                        {
                            XName name = XNamespace.Get(RequiredNamespaces[prop.NamespaceName]) + prop.SvgName;
                            element.Add(new XAttribute(name, prop.Unit.ToXml()));
                        }
                        else
                        {
                            element.Add(new XAttribute(prop.SvgName, prop.Unit.ToXml()));
                        }
                    }
                }
            }
        }

        if (this is IElementContainer container)
        {
            foreach (SvgElement child in container.Children)
            {
                element.Add(child.ToXml(nameSpace));
            }
        }

        return element;
    }

    public virtual void ParseData(XmlReader reader)
    {
        // This is supposed to be overriden by child classes
        throw new SvgParsingException($"Element {TagName} does not support parsing");
    }

    protected void ParseAttributes(List<SvgProperty> properties, XmlReader reader)
    {
        if (!properties.Contains(Style))
        {
            properties.Insert(0, Style);
        }

        do
        {
            SvgProperty matchingProperty = properties.FirstOrDefault(x =>
                string.Equals(x.SvgName, reader.Name, StringComparison.OrdinalIgnoreCase));
            if (matchingProperty != null)
            {
                ParseAttribute(matchingProperty, reader);
            }
        } while (reader.MoveToNextAttribute());
    }

    private void ParseAttribute(SvgProperty property, XmlReader reader)
    {
        if (property is SvgList list)
        {
            ParseListProperty(list, reader);
        }
        else
        {
            property.Unit ??= property.CreateDefaultUnit();
            property.Unit.ValuesFromXml(reader.Value);
        }
    }

    private void ParseListProperty(SvgList list, XmlReader reader)
    {
        list.Unit ??= list.CreateDefaultUnit();
        list.Unit.ValuesFromXml(reader.Value);
    }
}
