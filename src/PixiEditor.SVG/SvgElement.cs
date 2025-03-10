using System.Text;
using System.Xml;
using System.Xml.Linq;
using PixiEditor.SVG.Elements;
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

    public XElement ToXml(XNamespace nameSpace, DefStorage defs)
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
                        element.Value = prop.Unit.ToXml(defs);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(prop.NamespaceName))
                        {
                            XName name = XNamespace.Get(RequiredNamespaces[prop.NamespaceName]) + prop.SvgName;
                            element.Add(new XAttribute(name, prop.Unit.ToXml(defs)));
                        }
                        else
                        {
                            element.Add(new XAttribute(prop.SvgName, prop.Unit.ToXml(defs)));
                        }
                    }
                }
            }
        }

        if (this is IElementContainer container)
        {
            foreach (SvgElement child in container.Children)
            {
                element.Add(child.ToXml(nameSpace, defs));
            }
        }

        return element;
    }

    public virtual void ParseData(XmlReader reader, SvgDefs defs)
    {
        // This is supposed to be overriden by child classes
        throw new SvgParsingException($"Element {TagName} does not support parsing");
    }

    /// <summary>
    /// Gets unit for property. If property does not have unit, it will try to get it from inlined style.
    /// </summary>
    /// <param name="forProperty">Property to get unit for</param>
    /// <param name="defs">Optional defs element to get units from</param>
    /// <typeparam name="TUnit">Type of unit to get</typeparam>
    /// <returns>Unit for property</returns>
    public TUnit? GetUnit<TUnit>(SvgProperty<TUnit> forProperty, SvgDefs defs = default)
        where TUnit : struct, ISvgUnit
    {
        if (forProperty.Unit != null) return forProperty.Unit.Value;

        if (Style.Unit != null)
        {
            var styleProp = Style.Unit.Value.TryGetStyleFor<SvgProperty<TUnit>, TUnit>(forProperty.SvgName, defs);
            if (styleProp != null && styleProp.Unit != null)
            {
                return styleProp.Unit.Value;
            }
        }

        return null;
    }

    protected void ParseAttributes(List<SvgProperty> properties, XmlReader reader, SvgDefs defs)
    {
        if (!properties.Contains(Id))
        {
            properties.Insert(0, Id);
        }

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
                ParseAttribute(matchingProperty, reader, defs);
            }
        } while (reader.MoveToNextAttribute());
    }

    private void ParseAttribute(SvgProperty property, XmlReader reader, SvgDefs defs)
    {
        if (property is SvgList list)
        {
            ParseListProperty(list, reader, defs);
        }
        else
        {
            property.Unit ??= property.CreateDefaultUnit();
            property.Unit.ValuesFromXml(reader.Value, defs);
        }
    }

    private void ParseListProperty(SvgList list, XmlReader reader, SvgDefs defs)
    {
        list.Unit ??= list.CreateDefaultUnit();
        list.Unit.ValuesFromXml(reader.Value, defs);
    }
}
