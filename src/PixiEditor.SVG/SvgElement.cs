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
                    if (!string.IsNullOrEmpty(prop.NamespaceName) && !string.IsNullOrEmpty(prop.NamespaceUri))
                    {
                        XAttribute nsAttribute = new XAttribute(XNamespace.Xmlns + prop.NamespaceName, prop.NamespaceUri);
                        element.Add(nsAttribute);
                        
                        XName name = XNamespace.Get(prop.NamespaceUri) + prop.SvgName;
                        element.Add(new XAttribute(name, prop.Unit.ToXml()));
                    }
                    else
                    {
                        element.Add(new XAttribute(prop.SvgName, prop.Unit.ToXml()));
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
            property.Unit ??= CreateDefaultUnit(property);
            property.Unit.ValuesFromXml(reader.Value);
        }
    }
    
    private void ParseListProperty(SvgList list, XmlReader reader)
    {
        list.Unit ??= CreateDefaultUnit(list);
        list.Unit.ValuesFromXml(reader.Value);
    }

    private ISvgUnit CreateDefaultUnit(SvgProperty property)
    {
        var genericType = property.GetType().GetGenericArguments();
        if (genericType.Length == 0)
        {
            throw new InvalidOperationException("Property does not have a generic type");
        }

        ISvgUnit unit = Activator.CreateInstance(genericType[0]) as ISvgUnit;
        if (unit == null)
        {
            throw new InvalidOperationException("Could not create unit");
        }

        return unit;
    }
}
