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

    private Edges thickness;
    private Color color;
    private Edges cornerRadius;
    private Edges padding;
    private Edges margin;

    private Color backgroundColor;
    private double width = double.NaN;
    private double height = double.NaN;

    public Color Color { get => color; set => SetField(ref color, value); }
    public Edges Thickness { get => thickness; set => SetField(ref thickness, value); }
    public Edges CornerRadius { get => cornerRadius; set => SetField(ref cornerRadius, value); }
    public Edges Padding { get => padding; set => SetField(ref padding, value); }
    public Edges Margin { get => margin; set => SetField(ref margin, value); }
    public Color BackgroundColor { get => backgroundColor; set => SetField(ref backgroundColor, value); }
    public double Width { get => width; set => SetField(ref width, value); }
    public double Height { get => height; set => SetField(ref height, value); }

    protected override Control CreateNativeControl()
    {
        border = new Avalonia.Controls.Border();

        border.ClipToBounds = true;

        if (Child != null)
        {
            border.Child = Child.BuildNative();
        }

        Binding colorBinding = new Binding()
        {
            Source = this, Path = nameof(Color), Converter = new ColorToAvaloniaBrushConverter()
        };

        Binding edgesBinding = new Binding()
        {
            Source = this, Path = nameof(Thickness), Converter = new EdgesToThicknessConverter()
        };

        Binding cornerRadiusBinding = new Binding()
        {
            Source = this, Path = nameof(CornerRadius), Converter = new EdgesToCornerRadiusConverter()
        };

        Binding paddingBinding = new Binding()
        {
            Source = this, Path = nameof(Padding), Converter = new EdgesToThicknessConverter()
        };

        Binding marginBinding = new Binding()
        {
            Source = this, Path = nameof(Margin), Converter = new EdgesToThicknessConverter()
        };

        Binding backgroundColorBinding = new Binding()
        {
            Source = this, Path = nameof(BackgroundColor), Converter = new ColorToAvaloniaBrushConverter()
        };

        Binding widthBinding = new Binding() { Source = this, Path = nameof(Width), };

        Binding heightBinding = new Binding() { Source = this, Path = nameof(Height), };

        border.Bind(Layoutable.WidthProperty, widthBinding);
        border.Bind(Layoutable.HeightProperty, heightBinding);
        border.Bind(Avalonia.Controls.Border.BackgroundProperty, backgroundColorBinding);
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
        yield return BackgroundColor;
        yield return Width;
        yield return Height;
    }

    public void DeserializeProperties(ImmutableList<object> values)
    {
        Color = (Color)values.ElementAtOrDefault(0, default(Color));
        Thickness = (Edges)values.ElementAtOrDefault(1, default(Edges));
        CornerRadius = (Edges)values.ElementAtOrDefault(2, default(Edges));
        Padding = (Edges)values.ElementAtOrDefault(3, default(Edges));
        Margin = (Edges)values.ElementAtOrDefault(4, default(Edges));
        Width = (double)values.ElementAtOrDefault(5, double.NaN);
        Height = (double)values.ElementAtOrDefault(6, double.NaN);
        BackgroundColor = (Color)values.ElementAtOrDefault(7, default(Color));

        Width = Width < 0 ? double.NaN : Width;
        Height = Height < 0 ? double.NaN : Height;
    }
}
