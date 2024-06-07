using System.Collections.Immutable;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using PixiEditor.Extensions.CommonApi.FlyUI.Properties;
using PixiEditor.Extensions.Extensions;
using PixiEditor.Extensions.FlyUI.Converters;

namespace PixiEditor.Extensions.FlyUI.Elements;

public class Padding : SingleChildLayoutElement, IPropertyDeserializable
{
    private Edges _edges = Edges.All(0);
    
    public Edges Edges { get => _edges; set => SetField(ref _edges, value); }
    public override Control BuildNative()
    {
        Decorator decorator = new();
        
        if(Child != null)
        {
            decorator.Child = Child.BuildNative();
        }
        
        Binding edgesBinding = new()
        {
            Source = this,
            Path = nameof(Edges),
            Converter = new EdgesToThicknessConverter(),
        };
        
        decorator.Bind(Decorator.PaddingProperty, edgesBinding);
        
        return decorator;
    }

    public IEnumerable<object> GetProperties()
    {
        yield return Edges;
    }

    public void DeserializeProperties(ImmutableList<object> values)
    {
        Edges = (Edges)values.ElementAtOrDefault(0, default(Edges));
    }
}
