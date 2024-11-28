using System.Text;
using PixiEditor.SVG.Features;
using PixiEditor.SVG.Units;

namespace PixiEditor.SVG;

public class SvgElement(string tagName)
{
    public SvgProperty<SvgStringUnit> Id { get; } = new("id");
    public Dictionary<string, string> RequiredNamespaces { get; } = new();
    public string TagName { get; } = tagName;

    public string ToXml()
    {
        StringBuilder builder = new();
        builder.Append($"<{TagName}");

        foreach (var property in GetType().GetProperties())
        {
            if (property.PropertyType.IsAssignableTo(typeof(SvgProperty)))
            {
                SvgProperty prop = (SvgProperty)property.GetValue(this);
                if (prop != null)
                {
                    if (prop.Unit != null)
                    {
                        builder.Append($" {prop.SvgName}=\"{prop.Unit.ToXml()}\"");
                    }
                }
            }
        }
        
        if (this is not IElementContainer container)
        {
            builder.Append(" />");
        }
        else
        {
            builder.Append(">");
            foreach (SvgElement child in container.Children)
            {
                builder.AppendLine(child.ToXml());
            }
            
            builder.Append($"</{TagName}>");
        }

        return builder.ToString();
    }
}
