using System.Collections.Immutable;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using PixiEditor.Extensions.CommonApi.FlyUI.Properties;
using PixiEditor.Extensions.Extensions;
using PixiEditor.Extensions.FlyUI.Converters;

namespace PixiEditor.Extensions.FlyUI.Elements;

public class Border : SingleChildLayoutElement, IPropertyDeserializable
{
    private Avalonia.Controls.Border border;
    
    private Edges _thickness;
    private Color _color;
    private Edges cornerRadius;
    private Edges padding;
    private Edges margin;
    
    public Color Color { get => _color; set => SetField(ref _color, value); }
    public Edges Thickness { get => _thickness; set => SetField(ref _thickness, value); }
    public Edges CornerRadius { get => cornerRadius; set => SetField(ref cornerRadius, value); }
    public Edges Padding { get => padding; set => SetField(ref padding, value); }
    public Edges Margin { get => margin; set => SetField(ref margin, value); }
    
    public override Control BuildNative()
    {
        border = new Avalonia.Controls.Border();
        
        border.ClipToBounds = true;
        
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
            Path = nameof(Thickness),
            Converter = new EdgesToThicknessConverter()
        };
        
        Binding cornerRadiusBinding = new Binding()
        {
            Source = this,
            Path = nameof(CornerRadius),
            Converter = new EdgesToCornerRadiusConverter()
        };
        
        Binding paddingBinding = new Binding()
        {
            Source = this,
            Path = nameof(Padding),
            Converter = new EdgesToThicknessConverter()
        };
        
        Binding marginBinding = new Binding()
        {
            Source = this,
            Path = nameof(Margin),
            Converter = new EdgesToThicknessConverter()
        };
        
        border.Bind(Avalonia.Controls.Border.BorderBrushProperty, colorBinding);
        border.Bind(Avalonia.Controls.Border.BorderThicknessProperty, edgesBinding);
        border.Bind(Avalonia.Controls.Border.CornerRadiusProperty, cornerRadiusBinding);
        border.Bind(Decorator.PaddingProperty, paddingBinding);
        border.Bind(Layoutable.MarginProperty, marginBinding);
        
        return border;
    }

    protected override void AddChild(Control child)
    {
        border.Child = child;
    }

    protected override void RemoveChild()
    {
        border.Child = null;
    }

    public IEnumerable<object> GetProperties()
    {
        yield return Color;
        yield return Thickness;
        yield return CornerRadius;
        yield return Padding;
        yield return Margin;
    }

    public void DeserializeProperties(ImmutableList<object> values)
    {
        Color = (Color)values.ElementAtOrDefault(0, default(Color));
        Thickness = (Edges)values.ElementAtOrDefault(1, default(Edges));
        CornerRadius = (Edges)values.ElementAtOrDefault(2, default(Edges));
        Padding = (Edges)values.ElementAtOrDefault(3, default(Edges));
        Margin = (Edges)values.ElementAtOrDefault(4, default(Edges));
    }
}
