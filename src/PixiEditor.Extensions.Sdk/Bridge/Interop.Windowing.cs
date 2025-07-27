using PixiEditor.Extensions.CommonApi.Windowing;
using PixiEditor.Extensions.Sdk.Api.Window;

namespace PixiEditor.Extensions.Sdk.Bridge;

internal static partial class Interop
{
    private static Dictionary<int, List<Action<PopupWindow>>> callbacks =
        new Dictionary<int, List<Action<PopupWindow>>>();

    public static void RegisterWindowOpenedCallback(int type, Action<PopupWindow> callback)
    {
        if (!callbacks.ContainsKey(type))
        {
            callbacks[type] = new List<Action<PopupWindow>>();
        }

        callbacks[type].Add(callback);
        Native.subscribe_built_in_window_opened(type);
    }

    public static void OnBuiltInWindowOpened(int type, int handle)
    {
        if (callbacks.TryGetValue(type, out var callback))
        {
            PopupWindow window = new PopupWindow(handle);
            foreach (var action in callback)
            {
                action?.Invoke(window);
            }
        }
    }
}
