using PixiEditor.Extensions.CommonApi.FlyUI.Properties;
using PixiEditor.Extensions.CommonApi.Windowing;
using PixiEditor.Extensions.Sdk;
using PixiEditor.Extensions.Sdk.Api.FlyUI;
using PixiEditor.Extensions.Sdk.Api.Window;

namespace Sample10_VisualTree;

public class VisualTreeSampleExtension : PixiEditorExtension
{
    /// <summary>
    ///     This method is called when extension is loaded.
    ///  All extensions are first loaded and then initialized. This method is called before <see cref="OnInitialized"/>.
    /// </summary>
    public override void OnLoaded()
    {
    }

    /// <summary>
    ///     This method is called when extension is initialized. After this method is called, you can use Api property to access PixiEditor API.
    /// </summary>
    public override void OnInitialized()
    {
        Api.WindowProvider.SubscribeWindowOpened(BuiltInWindowType.StartupWindow, InjectButton);
    }

    private void InjectButton(PopupWindow window)
    {
        var button = new Button(new Text("Click me!"));

        var element = Api.VisualTreeProvider.FindElement("ExampleFilesGrid", window);

        if (element is NativeMultiChildElement panel)
        {
            panel.AppendChild(0, button);
        }
    }

    public override void OnMainWindowLoaded()
    {
       // wip
    }
}