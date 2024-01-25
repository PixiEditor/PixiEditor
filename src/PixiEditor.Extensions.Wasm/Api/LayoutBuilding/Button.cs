using PixiEditor.Extensions.CommonApi.LayoutBuilding;

namespace PixiEditor.Extensions.Wasm.Api.LayoutBuilding;

public class Button : ISingleChildLayoutElement<NativeControl>
{
    ILayoutElement<NativeControl> ISingleChildLayoutElement<NativeControl>.Child
    {
        get => Content;
        set => Content = value;
    }

    public ILayoutElement<NativeControl> Content { get; set; }

    public Button(ILayoutElement<NativeControl> child = null)
    {
        Content = child;
    }

    public NativeControl Build()
    {
        NativeControl button = new NativeControl("Button");
        button.AddChild(Content.Build());
        return button;
    }
}
