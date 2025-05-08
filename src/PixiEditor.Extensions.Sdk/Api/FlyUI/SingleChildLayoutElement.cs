using PixiEditor.Extensions.CommonApi.FlyUI;

namespace PixiEditor.Extensions.Sdk.Api.FlyUI;

public abstract class SingleChildLayoutElement : LayoutElement, ISingleChildLayoutElement<ControlDefinition>
{
    public ILayoutElement<ControlDefinition> Child { get; set; }
}
