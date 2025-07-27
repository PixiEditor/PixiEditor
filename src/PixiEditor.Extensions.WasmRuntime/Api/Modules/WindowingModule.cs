using PixiEditor.Extensions.CommonApi.Windowing;

namespace PixiEditor.Extensions.WasmRuntime.Api.Modules;

public class WindowingModule(WasmExtensionInstance extension) : ApiModule(extension)
{
    public void SubscribeBuiltInWindowOpened(int type)
    {
        BuiltInWindowType windowType = (BuiltInWindowType)type;
        Extension.Api.Windowing.SubscribeWindowOpened(windowType, (window) =>
        {
            int handle = Extension.NativeObjectManager.AddObject(window);
            Extension.Instance.GetAction<int, int>("on_built_in_window_opened").Invoke(type, handle);
        });
    }
}
