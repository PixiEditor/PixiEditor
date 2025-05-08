using PixiEditor.Extensions.CommonApi.FlyUI;

namespace PixiEditor.Extensions.Sdk.Api.FlyUI;

public abstract class MultiChildLayoutElement : LayoutElement, IMultiChildLayoutElement<ControlDefinition>
{
    List<ILayoutElement<ControlDefinition>> IMultiChildLayoutElement<ControlDefinition>.Children
    {
        get => Children.Cast<ILayoutElement<ControlDefinition>>().ToList();
        set => Children = value.Cast<LayoutElement>().ToList();
    }

    public List<LayoutElement> Children { get; set; }


}
