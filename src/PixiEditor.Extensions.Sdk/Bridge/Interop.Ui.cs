using PixiEditor.Extensions.CommonApi.FlyUI;
using PixiEditor.Extensions.CommonApi.Windowing;
using PixiEditor.Extensions.Sdk.Api.FlyUI;
using PixiEditor.Extensions.Sdk.Api.Window;
using PixiEditor.Extensions.Sdk.Utilities;

namespace PixiEditor.Extensions.Sdk.Bridge;

internal partial class Interop
{
    private static Dictionary<string, Type> typeMap;

    public static void SubscribeToEvents(ControlDefinition body)
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

    /// <summary>
    /// Finds an element in the visual tree by its name.
    /// </summary>
    /// <param name="name">The name of the element to find.</param>
    /// <returns>A byte array representing the found element, or null if not found.</returns>
    public static LayoutElement? FindUiElement(string name)
    {
        int id = LayoutElementIdGenerator.CurrentId + 1;
        var element = InteropUtility.PrefixedIntPtrToByteArray(Native.find_ui_element(name, id));
        if (element.Length == 0)
        {
            return null;
        }

        int typeLength = BitConverter.ToInt32(element, 0);
        string typeId = System.Text.Encoding.UTF8.GetString(element, 4, typeLength);

        if (!typeMap.TryGetValue(typeId, out Type? type))
        {
            type = typeof(NativeElement);
        }

        LayoutElement lElem = (LayoutElement)Activator.CreateInstance(type, (Cursor?)null)!;

        return lElem;
    }

    public static LayoutElement FindUiElement(string name, PopupWindow root)
    {
        int id = LayoutElementIdGenerator.CurrentId + 1;
        var element = InteropUtility.PrefixedIntPtrToByteArray(Native.find_ui_element_in_popup(name, root.Handle, id));
        if (element.Length == 0)
        {
            return null;
        }

        int typeLength = BitConverter.ToInt32(element, 0);
        string typeId = System.Text.Encoding.UTF8.GetString(element, 4, typeLength);

        if (!typeMap.TryGetValue(typeId, out Type? type))
        {
            type = typeof(NativeElement);
        }

        LayoutElement lElem = (LayoutElement)Activator.CreateInstance(type, (Cursor?)null)!;

        return lElem;
    }

    public static void AppendElementToNativeMultiChild(int uniqueId, LayoutElement element, int atIndex)
    {
        if (element == null)
            return;

        var built = element.BuildNative();
        var bytes = built.SerializeBytes();
        int bodyLen = bytes.Length;

        Native.append_element_to_native_multi_child(atIndex, uniqueId, InteropUtility.ByteArrayToIntPtr(bytes),
            bodyLen);

        SubscribeToEvents(built);
    }
}
