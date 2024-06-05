using PixiEditor.Extensions.CommonApi.FlyUI.Properties;
using PixiEditor.Extensions.Wasm.Bridge;

namespace PixiEditor.Extensions.Wasm.Api.FlyUI;

public class Image : StatelessElement
{
    private string source = null!;

    public string Source
    {
        get => source;
        set
        {
            if (value.StartsWith("/") || value.StartsWith("/Resources/") || value.StartsWith("Resources/"))
            {
                source = Native.to_resources_full_path(value);    
            }
            else
            {
                source = value;
            }
        }
    }

    public double Width { get; set; } 
    public double Height { get; set; }
    public FillMode FillMode { get; set; }
    public FilterQuality FilterQuality { get; set; }

    public Image(string source, double width = -1, double height = -1, FillMode fillMode = FillMode.Uniform, FilterQuality filterQuality = FilterQuality.Unspecified)
    {
        Source = source;
        Width = width;
        Height = height;
        FillMode = fillMode;
        FilterQuality = filterQuality;
    }

    public override CompiledControl BuildNative()
    {
        CompiledControl image = new CompiledControl(UniqueId, "Image");
        
        image.AddStringProperty(Source);
        image.AddProperty(Width);
        image.AddProperty(Height);
        image.AddProperty((int)FillMode);
        image.AddProperty((byte)FilterQuality);
        
        BuildPendingEvents(image);
        return image;
    }
}
