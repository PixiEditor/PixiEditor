using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using PixiEditor.Extensions.FlyUI.Converters;

namespace PixiEditor.Extensions.FlyUI.Elements;

public class Image : StatelessElement, IPropertyDeserializable
{
    private string _source = null!;
    public string Source { get => _source; set => SetField(ref _source, value); }
    
    public override Control BuildNative()
    {
        Avalonia.Controls.Image image = new();
        
        Binding sourceBinding = new()
        {
            Source = this,
            Path = nameof(Source),
            Converter = new PathToBitmapConverter(),
        };
        
        image.Bind(Avalonia.Controls.Image.SourceProperty, sourceBinding);
        
        return image;
    }


    public IEnumerable<object> GetProperties()
    {
        yield return Source;
    }

    public void DeserializeProperties(IEnumerable<object> values)
    {
        Source = (string)values.ElementAt(0);
    }
}
