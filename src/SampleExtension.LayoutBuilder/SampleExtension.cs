using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using PixiEditor.Extensions;
using PixiEditor.Extensions.LayoutBuilding.Elements;
using PixiEditor.Extensions.Windowing;

namespace SampleExtension.LayoutBuilder;

public class SampleExtension : Extension
{
    protected override void OnLoaded()
    {
    }

    protected override void OnInitialized()
    {
        Layout layout = new Layout(
            body: new ButtonTextElement());

        Api.Windowing.CreatePopupWindow("Test layout builder", layout.BuildNative()).Show();
    }
}
