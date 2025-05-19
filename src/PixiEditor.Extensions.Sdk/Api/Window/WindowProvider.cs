using System.Runtime.InteropServices;
using PixiEditor.Extensions.CommonApi.Windowing;
using PixiEditor.Extensions.Sdk.Api.FlyUI;
using PixiEditor.Extensions.Sdk.Bridge;

namespace PixiEditor.Extensions.Sdk.Api.Window;

public class WindowProvider : IWindowProvider
{
    public PopupWindow CreatePopupWindow(string title, LayoutElement body)
    {
        ControlDefinition controlDefinition = body.BuildNative();
        byte[] bytes = controlDefinition.Serialize().ToArray();
        IntPtr ptr = Marshal.AllocHGlobal(bytes.Length);
        Marshal.Copy(bytes, 0, ptr, bytes.Length);
        int handle = Native.create_popup_window(title, ptr, bytes.Length);
        Marshal.FreeHGlobal(ptr);
        
        SubscribeToEvents(controlDefinition);
        return new PopupWindow(handle);
    }

    internal void LayoutStateChanged(int uniqueId, ControlDefinition newLayout)
    {
        byte[] bytes = newLayout.Serialize().ToArray();
        IntPtr ptr = Marshal.AllocHGlobal(bytes.Length);
        Marshal.Copy(bytes, 0, ptr, bytes.Length);
        Native.state_changed(uniqueId, ptr, bytes.Length);
        Marshal.FreeHGlobal(ptr);

        SubscribeToEvents(newLayout);
    }

    private void SubscribeToEvents(ControlDefinition body)
    {
        foreach (ControlDefinition child in body.Children)
        {
            SubscribeToEvents(child);
        }

        foreach (var queuedEvent in body.QueuedEvents)
        {
            Native.subscribe_to_event(body.UniqueId, queuedEvent);
        }
    }

    public IPopupWindow CreatePopupWindow(string title, object body)
    {
        if(body is not LayoutElement element)
            throw new ArgumentException("Body must be of type LayoutElement");

        return CreatePopupWindow(title, element);
    }

    public IPopupWindow GetWindow(BuiltInWindowType type)
    {
        int handle = Native.get_built_in_window((int)type);
        return new PopupWindow(handle);
    }

    public IPopupWindow GetWindow(string windowId)
    {
        int handle = Native.get_window(windowId);
        return new PopupWindow(handle);
    }
}
