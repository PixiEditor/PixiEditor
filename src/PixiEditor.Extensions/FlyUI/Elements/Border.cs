using System.Collections.Immutable;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using PixiEditor.Extensions.CommonApi.FlyUI.Properties;
using PixiEditor.Extensions.Extensions;
using PixiEditor.Extensions.FlyUI.Converters;

namespace PixiEditor.Extensions.FlyUI.Elements;

public class Border : SingleChildLayoutElement, IPropertyDeserializable
{
    private Edges _edges;
    private Color _color;
    
    public Color Color { get => _color; set => SetField(ref _color, value); }
    public Edges Edges { get => _edges; set => SetField(ref _edges, value); }
    
    public Border(LayoutElement child = null, Color color = default, Edges edges = default)
    {
        Child = child;
        _color = color;
        _edges = edges;
    }
    
    public override Control BuildNative()
    {
        Avalonia.Controls.Border border = new Avalonia.Controls.Border();
        
        if (Child != null)
        {
            border.Child = Child.BuildNative();
        }
        
        Binding colorBinding = new Binding()
        {
            Source = this,
            Path = nameof(Color),
            Converter = new ColorToAvaloniaBrushConverter()
        };
        
        Binding edgesBinding = new Binding()
        {
            Source = this,
            Path = nameof(Edges),
            Converter = new EdgesToThicknessConverter()
        };
        
        border.Bind(Avalonia.Controls.Border.BorderBrushProperty, colorBinding);
        border.Bind(Avalonia.Controls.Border.BorderThicknessProperty, edgesBinding);
        
        return border;
    }

    public IEnumerable<object> GetProperties()
    {
        yield return Color;
        yield return Edges;
    }

    public void DeserializeProperties(ImmutableList<object> values)
    {
        Color = (Color)values.ElementAtOrDefault(0, default(Color));
        Edges = (Edges)values.ElementAtOrDefault(1, default(Edges));
    }
}
