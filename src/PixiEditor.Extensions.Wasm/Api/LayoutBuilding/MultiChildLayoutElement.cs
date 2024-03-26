using PixiEditor.Extensions.CommonApi.LayoutBuilding;

namespace PixiEditor.Extensions.Wasm.Api.LayoutBuilding;

public abstract class MultiChildLayoutElement : LayoutElement, IMultiChildLayoutElement<CompiledControl>
{
    List<ILayoutElement<CompiledControl>> IMultiChildLayoutElement<CompiledControl>.Children
    {
        get => Children.Cast<ILayoutElement<CompiledControl>>().ToList();
        set => Children = value.Cast<LayoutElement>().ToList();
    }

    public List<LayoutElement> Children { get; set; }

    public abstract override CompiledControl BuildNative();

}
