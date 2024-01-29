using Avalonia.Controls;
using PixiEditor.Extensions.CommonApi.LayoutBuilding;

namespace PixiEditor.Extensions.LayoutBuilding.Elements;

public abstract class SingleChildLayoutElement : LayoutElement, ISingleChildLayoutElement<Control>
{
    public ILayoutElement<Control>? Child { get; set; }
    public abstract override Control BuildNative();
}
