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
    
    public Image(string source)
    {
        Source = source;
    }
    
    public override CompiledControl BuildNative()
    {
        CompiledControl image = new CompiledControl(UniqueId, "Image");
        
        image.AddStringProperty(Source);
        BuildPendingEvents(image);
        return image;
    }
}
