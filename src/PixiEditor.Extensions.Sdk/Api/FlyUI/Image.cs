using PixiEditor.Extensions.CommonApi.FlyUI.Properties;
using PixiEditor.Extensions.Sdk.Bridge;

namespace PixiEditor.Extensions.Sdk.Api.FlyUI;

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

    public override ControlDefinition BuildNative()
    {
        ControlDefinition image = new ControlDefinition(UniqueId, "Image");
        
        image.AddProperty(Source);
        image.AddProperty(Width);
        image.AddProperty(Height);
        image.AddProperty(FillMode);
        image.AddProperty(FilterQuality);
        
        BuildPendingEvents(image);
        return image;
    }
}
