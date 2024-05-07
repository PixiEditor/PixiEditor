using Avalonia.Controls;
using PixiEditor.Extensions.CommonApi.FlyUI.Events;
using PixiEditor.Extensions.FlyUI.Elements;
using Button = PixiEditor.Extensions.FlyUI.Elements.Button;

namespace SampleExtension.LayoutBuilder;

public class ButtonTextElementState : State
{
    public int ClickedTimes { get; private set; } = 0;

    public override LayoutElement BuildElement()
    {
        return new Button(
            onClick: OnClick,
            child: new Text($"Cassd: {ClickedTimes}"));
    }

    private void OnClick(ElementEventArgs args)
    {
        SetState(() => ClickedTimes++);
    }
}
