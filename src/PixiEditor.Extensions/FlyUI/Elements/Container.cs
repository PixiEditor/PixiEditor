using System.Collections.Immutable;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using PixiEditor.Extensions.CommonApi.FlyUI.Properties;
using PixiEditor.Extensions.Extensions;
using PixiEditor.Extensions.FlyUI.Converters;

namespace PixiEditor.Extensions.FlyUI.Elements;

public class Container : SingleChildLayoutElement, IPropertyDeserializable
{
    private Edges _margin = Edges.All(0);
    private Color _backgroundColor = Colors.Transparent;
    private double _width = double.NaN;
    private double _height = double.NaN;
    private Edges _cornerRadius = Edges.All(0);
    public Edges Margin { get => _margin; set => SetField(ref _margin, value); }
    public Color BackgroundColor { get => _backgroundColor; set => SetField(ref _backgroundColor, value); }
    public double Width { get => _width; set => SetField(ref _width, value); }
    public double Height { get => _height; set => SetField(ref _height, value); }
    
    public override Control BuildNative()
    {
        Panel panel = new Panel();
        
        panel.ClipToBounds = true;
        
        if(Child != null)
        {
            panel.Children.Add(Child.BuildNative());
        }
        
        Binding marginBinding = new()
        {
            Source = this,
            Path = nameof(Margin),
            Converter = new EdgesToThicknessConverter(),
        };
        
        Binding backgroundColorBinding = new()
        {
            Source = this,
            Path = nameof(BackgroundColor),
            Converter = new FlyUiColorToBrushConverter(),
        };
        
        Binding widthBinding = new()
        {
            Source = this,
            Path = nameof(Width),
        };
        
        Binding heightBinding = new()
        {
            Source = this,
            Path = nameof(Height),
        };
        
        panel.Bind(Layoutable.MarginProperty, marginBinding);
        panel.Bind(Panel.BackgroundProperty, backgroundColorBinding);
        panel.Bind(Layoutable.WidthProperty, widthBinding);
        panel.Bind(Layoutable.HeightProperty, heightBinding);
        
        return panel;
    }

    public IEnumerable<object> GetProperties()
    {
        yield return Margin.Left;
        yield return Margin.Top;
        yield return Margin.Right;
        yield return Margin.Bottom;
        
        yield return BackgroundColor.R;
        yield return BackgroundColor.G;
        yield return BackgroundColor.B;
        yield return BackgroundColor.A;
        
        yield return Width;
        yield return Height;
    }

    public void DeserializeProperties(ImmutableList<object> values)
    {
        Margin = (Edges)values.ElementAtOrDefault(0, default(Edges));
        BackgroundColor = (Color)values.ElementAtOrDefault(1, default(Color));
        Width = (double)values.ElementAtOrDefault(2, double.NaN);
        Height = (double)values.ElementAtOrDefault(3, double.NaN);
        
        Width = Width < 0 ? double.NaN : Width;
        Height = Height < 0 ? double.NaN : Height;
    }
}
