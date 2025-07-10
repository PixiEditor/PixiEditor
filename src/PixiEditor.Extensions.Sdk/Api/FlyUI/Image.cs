using PixiEditor.Extensions.CommonApi.FlyUI;
using PixiEditor.Extensions.CommonApi.FlyUI.Properties;
using PixiEditor.Extensions.Sdk.Attributes;
using PixiEditor.Extensions.Sdk.Bridge;

namespace PixiEditor.Extensions.Sdk.Api.FlyUI;

[ControlTypeId("Image")]
public class Image : LayoutElement
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

    public Image(string source, double width = -1, double height = -1, FillMode fillMode = FillMode.Uniform, FilterQuality filterQuality = FilterQuality.Unspecified, Cursor? cursor = null) : base(cursor)
    {
        Source = source;
        Width = width;
        Height = height;
        FillMode = fillMode;
        FilterQuality = filterQuality;
    }

    protected override ControlDefinition CreateControl()
    {
        ControlDefinition image = new ControlDefinition(UniqueId, GetType());
        
        image.AddProperty(Source);
        image.AddProperty(Width);
        image.AddProperty(Height);
        image.AddProperty(FillMode);
        image.AddProperty(FilterQuality);
        
        return image;
    }
}
