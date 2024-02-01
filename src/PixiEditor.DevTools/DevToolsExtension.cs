using PixiEditor.DevTools.Layouts;
using PixiEditor.Extensions;

namespace PixiEditor.DevTools;

public class DevToolsExtension : Extension
{
    public static ExtensionServices PixiEditorApi { get; private set; } = null!;
    protected override void OnInitialized()
    {
        PixiEditorApi = Api;
        Api.Windowing.CreatePopupWindow("Elements UI Builder", new LiveLayoutPreviewWindow().BuildNative()).Show();
    }
}
