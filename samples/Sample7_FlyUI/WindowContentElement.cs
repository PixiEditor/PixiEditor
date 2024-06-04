using PixiEditor.Extensions.Wasm.Api.FlyUI;
using PixiEditor.Extensions.Wasm.Api.Localization;

namespace FlyUISample;

public class WindowContentElement : StatelessElement
{
    public override CompiledControl BuildNative()
    {
        Layout layout = new Layout(body:
            new Column(
                new Center(
                    new Text("Hello there!")
                ),
                new Text("This is a sample window content element."))
        );

        return layout.BuildNative();
    }
}