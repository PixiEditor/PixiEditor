using PixiEditor.SVG.Elements;

namespace PixiEditor.SVG.Units;

public struct SvgStyleUnit : ISvgUnit
{
    private Dictionary<string, string> inlineDefinedProperties;
    private string value;

    public SvgStyleUnit(string inlineStyle)
    {
        Value = inlineStyle;
    }

    public string Value
    {
        get => value;
        set
        {
            this.value = value;
            inlineDefinedProperties = new Dictionary<string, string>();

            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            string[] properties = value.Split(';');
            foreach (string property in properties)
            {
                string[] keyValue = property.Split(':');
                if (keyValue.Length == 2)
                {
                    inlineDefinedProperties.Add(keyValue[0].Trim(), keyValue[1].Trim());
                }
            }
        }
    }

    public string ToXml(DefStorage defs)
    {
        return Value;
    }

    public void ValuesFromXml(string readerValue, SvgDefs defs)
    {
        Value = readerValue;
    }

    public TProp TryGetStyleFor<TProp, TUnit>(string property, SvgDefs defs) where TProp : SvgProperty<TUnit> where TUnit : struct, ISvgUnit
    {
        if (inlineDefinedProperties.TryGetValue(property, out var definedProperty))
        {
            TProp prop = (TProp)Activator.CreateInstance(typeof(TProp), property);
            var unit = (TUnit)prop.CreateDefaultUnit();
            unit.ValuesFromXml(definedProperty, defs);
            prop.Unit = unit;

            return prop;
        }

        return null;
    }

    public SvgStyleUnit MergeWith(SvgStyleUnit elementStyleUnit)
    {
        Dictionary<string, string> props = new(inlineDefinedProperties);
        foreach (var inlineDefined in elementStyleUnit.inlineDefinedProperties)
        {
            props[inlineDefined.Key] = inlineDefined.Value;
        }

        return new SvgStyleUnit(string.Join(";", props.Select(x => $"{x.Key}:{x.Value}")));
    }
}
