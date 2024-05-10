using PixiEditor.Extensions.CommonApi.FlyUI.Events;
using PixiEditor.Extensions.Wasm.Api.LayoutBuilding;

namespace WasmSampleExtension;

public class ButtonTextElementState : State
{
    public int ClickedTimes { get; private set; } = 0;

    public override LayoutElement BuildElement()
    {
        return new Button(
            onClick: OnClick,
            child: new Text($"Clicked: {ClickedTimes} times"));
    }

    private void OnClick(ElementEventArgs args)
    {
        SetState(() => ClickedTimes++);
    }
}
