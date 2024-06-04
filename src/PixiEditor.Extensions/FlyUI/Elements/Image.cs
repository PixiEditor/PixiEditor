using System.Collections.Immutable;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using PixiEditor.Extensions.CommonApi.FlyUI.Properties;
using PixiEditor.Extensions.Extensions;
using PixiEditor.Extensions.FlyUI.Converters;

namespace PixiEditor.Extensions.FlyUI.Elements;

public class Image : StatelessElement, IPropertyDeserializable
{
    private string _source = null!;
    private double _width = -1;
    private double _height = -1;
    private FillMode _fillMode = FillMode.Uniform;
    
    public string Source { get => _source; set => SetField(ref _source, value); }
    public double Width { get => _width; set => SetField(ref _width, value); }
    public double Height { get => _height; set => SetField(ref _height, value); }
    public FillMode FillMode { get => _fillMode; set => SetField(ref _fillMode, value); }
    
    public override Control BuildNative()
    {
        Avalonia.Controls.Image image = new();
        
        Binding sourceBinding = new()
        {
            Source = this,
            Path = nameof(Source),
            Converter = new PathToBitmapConverter(),
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
        
        Binding fillModeBinding = new()
        {
            Source = this,
            Path = nameof(FillMode),
            Converter = new EnumToEnumConverter<FillMode, Stretch>()
        };
        
        image.Bind(Avalonia.Controls.Image.SourceProperty, sourceBinding);
        image.Bind(Layoutable.WidthProperty, widthBinding);
        image.Bind(Layoutable.HeightProperty, heightBinding);
        image.Bind(Avalonia.Controls.Image.StretchProperty, fillModeBinding);
        
        return image;
    }


    public IEnumerable<object> GetProperties()
    {
        yield return Source;
        
        yield return Width;
        yield return Height;
        
        yield return (int)FillMode;
    }

    public void DeserializeProperties(ImmutableList<object> values)
    {
        var valuesList = values.ToList();
        Source = (string)valuesList.ElementAtOrDefault(0);
        
        Width = (double)valuesList.ElementAtOrDefault(1, double.NaN);
        Height = (double)valuesList.ElementAtOrDefault(2, double.NaN);
        
        Width = Width < 0 ? double.NaN : Width;
        Height = Height < 0 ? double.NaN : Height;
        
        FillMode = (FillMode)valuesList.ElementAtOrDefault(3, (int)FillMode.Uniform);
    }
}
