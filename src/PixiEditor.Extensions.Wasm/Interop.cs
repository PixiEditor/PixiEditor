using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using PixiEditor.Extensions.CommonApi.LayoutBuilding;
using PixiEditor.Extensions.CommonApi.LayoutBuilding.Events;
using PixiEditor.Extensions.Wasm.Api.LayoutBuilding;

namespace PixiEditor.Extensions.Wasm;

internal class Interop
{
    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern void LogMessage(string message);

    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern void CreatePopupWindow(string title, IntPtr data, int length);

    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern void SubscribeToEvent(int internalControlId, string eventName);

    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern void StateChanged(int uniqueId, IntPtr data, int length);

    internal static void Load()
    {
        Type extensionType = Assembly.GetEntryAssembly().ExportedTypes
            .FirstOrDefault(type => type.IsSubclassOf(typeof(WasmExtension)));

        Debug.Assert(extensionType != null, "extensionType != null");

        LogMessage($"Loading extension {extensionType.FullName}");

        WasmExtension extension = (WasmExtension)Activator.CreateInstance(extensionType);
        ExtensionContext.Active = extension;
        extension.OnLoaded();
    }

    internal static void Initialize()
    {
        ExtensionContext.Active.OnInitialized();
    }

    internal static void EventRaised(int internalControlId, string eventName) //TOOD: Args
    {
        if (LayoutElementsStore.LayoutElements.TryGetValue((int)internalControlId, out ILayoutElement<CompiledControl> element))
        {
            element.RaiseEvent(eventName ?? "", new ElementEventArgs());
        }
    }

    internal static void SetElementMap(byte[] bytes)
    {
        // Dictionary format: [int bytes controlTypeId, string controlTypeName]

        int offset = 0;
        while (offset < bytes.Length)
        {
            int id = BitConverter.ToInt32(bytes, offset);
            offset += sizeof(int);
            int nameLength = BitConverter.ToInt32(bytes, offset);
            offset += sizeof(int);
            string name = Encoding.UTF8.GetString(bytes, offset, nameLength);
            offset += nameLength;
            ByteMap.ControlMap.Add(name, id);
        }
    }
}
