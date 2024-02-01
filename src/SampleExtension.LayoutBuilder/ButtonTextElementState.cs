using Avalonia.Controls;
using PixiEditor.Extensions.CommonApi.LayoutBuilding;
using PixiEditor.Extensions.CommonApi.LayoutBuilding.Events;
using PixiEditor.Extensions.LayoutBuilding.Elements;
using Button = PixiEditor.Extensions.LayoutBuilding.Elements.Button;

namespace SampleExtension.LayoutBuilder;

public class ButtonTextElementState : State
{
    public int ClickedTimes { get; private set; } = 0;

    public override LayoutElement BuildElement()
    {
        return new Button(
            onClick: OnClick,
            child: new Text($"Clicked: {ClickedTimes}"));
    }

    private void OnClick(ElementEventArgs args)
    {
        SetState(() => ClickedTimes++);
    }
}
