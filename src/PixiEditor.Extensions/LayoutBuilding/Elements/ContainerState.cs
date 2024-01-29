using Avalonia.Controls;
using PixiEditor.Extensions.CommonApi.LayoutBuilding;

namespace PixiEditor.Extensions.LayoutBuilding.Elements;

public class ContainerState : State
{
    public ILayoutElement<Control> Content { get; set; }

    public override ILayoutElement<Control> Build()
    {
        return Content;
    }
}
