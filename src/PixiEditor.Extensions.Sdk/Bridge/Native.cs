using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using PixiEditor.Extensions.CommonApi.FlyUI;
using PixiEditor.Extensions.CommonApi.FlyUI.Events;
using PixiEditor.Extensions.Sdk.Api.FlyUI;
using PixiEditor.Extensions.Sdk.Utilities;
using ProtoBuf;

namespace PixiEditor.Extensions.Sdk.Bridge;

internal static class Program { internal static void Main() { } } // Required for compilation

[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:Element should begin with upper-case letter", 
    Justification = "Interop is a special case, it's injected to C code and follows C naming conventions.")]
internal static partial class Native
{
    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern void log_message(string message);


    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern unsafe void subscribe_to_event(int internalControlId, string eventName);

    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern void state_changed(int uniqueId, IntPtr data, int length);
    
    [MethodImpl(MethodImplOptions.InternalCall)]
    public static extern string get_extension_unique_name();


    // No need for [ApiExport] since this is a part of built-in C interop file.
    internal static void Load()
    {
        Type extensionType = Assembly.GetEntryAssembly().ExportedTypes
            .FirstOrDefault(type => type.IsSubclassOf(typeof(PixiEditorExtension)));

        Debug.Assert(extensionType != null, "extensionType != null");

        log_message($"Loading extension {extensionType.FullName}");

        PixiEditorExtension extension = (PixiEditorExtension)Activator.CreateInstance(extensionType);
        ExtensionContext.Active = extension;
        extension.OnLoaded();
    }

    // No need for [ApiExport] since this is a part of built-in C interop file.
    internal static void Initialize()
    {
        ExtensionContext.Active.OnInitialized();
    }

    [ApiExport("raise_element_event")]
    internal static void EventRaised(int internalControlId, string eventName, IntPtr eventData, int dataLength)
    {
        if (LayoutElementsStore.LayoutElements.TryGetValue((int)internalControlId, out ILayoutElement<ControlDefinition> element))
        {
            byte[] data = InteropUtility.IntPtrToByteArray(eventData, dataLength);
            ElementEventArgs args = ElementEventArgs.Deserialize(data);
            args.Sender = element;
            element.RaiseEvent(eventName ?? "", args);
        }
    }

    [ApiExport("raise_element_text_event")]
    internal static void TextEventRaised(int internalControlId, string eventName, string text)
    {
        if (LayoutElementsStore.LayoutElements.TryGetValue((int)internalControlId, out ILayoutElement<ControlDefinition> element))
        {
            element.RaiseEvent(eventName ?? "", new TextEventArgs(text) { Sender = element });
        }
    }

    [MethodImpl(MethodImplOptions.InternalCall)]
    public static extern string to_resources_full_path(string value);

}
