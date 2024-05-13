using System.Runtime.InteropServices;
using System.Text;
using Avalonia.Controls;
using Avalonia.Threading;
using PixiEditor.Extensions.FlyUI;
using PixiEditor.Extensions.FlyUI.Elements;
using PixiEditor.Extensions.WasmRuntime.Management;
using PixiEditor.Extensions.Windowing;
using Wasmtime;

namespace PixiEditor.Extensions.WasmRuntime;

public class WasmExtensionInstance : Extension
{
    public Instance? Instance { get; private set; }

    private Linker Linker { get; }
    private Store Store { get; }
    private Module Module { get; }

    private Memory memory = null!;
    private LayoutBuilder LayoutBuilder { get; set; }
    private ObjectManager NativeObjectManager { get; set; }
    private WasmMemoryUtility WasmMemoryUtility { get; set; }

    public WasmExtensionInstance(Linker linker, Store store, Module module)
    {
        Linker = linker;
        Store = store;
        Module = module;
    }

    public void Instantiate()
    {
        NativeObjectManager = new ObjectManager();
        DefinePixiEditorApi();
        Linker.DefineModule(Store, Module);

        Instance = Linker.Instantiate(Store, Module);
        WasmMemoryUtility = new WasmMemoryUtility(Instance);
        Instance.GetFunction("_start").Invoke();
        memory = Instance.GetMemory("memory");
    }

    protected override void OnInitialized()
    {
        LayoutBuilder = new LayoutBuilder((ElementMap)Api.Services.GetService(typeof(ElementMap)));

        //SetElementMap();
        Instance.GetAction("initialize").Invoke();
        base.OnInitialized();
    }

    protected override void OnLoaded()
    {
        Instance.GetAction("load").Invoke();
        base.OnLoaded();
    }

    private void SetElementMap()
    {
        var elementMap = (ElementMap)Api.Services.GetService(typeof(ElementMap));
        byte[] map = elementMap.Serialize();
        var ptr = WasmMemoryUtility.WriteSpan(map);
        Instance.GetAction<int, int>("set_element_map").Invoke(ptr, map.Length);

        WasmMemoryUtility.Free(ptr);
    }

    private void DefinePixiEditorApi()
    {
        Linker.DefineFunction("env", "log_message",(int messageOffset, int messageLength) =>
        {
            string messageString = WasmMemoryUtility.GetString(messageOffset, messageLength);
            Console.WriteLine(messageString.ReplaceLineEndings());
        });

        Linker.DefineFunction("env", "create_popup_window",(int titleOffset, int titleLength, int bodyOffset, int bodyLength) =>
        {
            string title = WasmMemoryUtility.GetString(titleOffset, titleLength);
            Span<byte> arr = memory.GetSpan<byte>(bodyOffset, bodyLength);

            var body = LayoutBuilder.Deserialize(arr, DuplicateResolutionTactic.ThrowException);

            var popupWindow = Api.Windowing.CreatePopupWindow(title, body.BuildNative());

            int handle = NativeObjectManager.AddObject(popupWindow);
            return WasmMemoryUtility.WriteInt32(handle);
        });

        Linker.DefineFunction("env", "set_window_title", (int handle, int titleOffset, int titleLength) =>
        {
            string title = WasmMemoryUtility.GetString(titleOffset, titleLength);
            var window = NativeObjectManager.GetObject<PopupWindow>(memory.ReadInt32(handle));
            window.Title = title;
        });

        Linker.DefineFunction("env", "get_window_title", (int handle) =>
        {
            var window = NativeObjectManager.GetObject<PopupWindow>(memory.ReadInt32(handle));
            return WasmMemoryUtility.WriteString(window.Title);
        });

        Linker.DefineFunction("env", "show_window", (int handle) =>
        {
            var window = NativeObjectManager.GetObject<PopupWindow>(memory.ReadInt32(handle));
            window.Show();
        });

        Linker.DefineFunction("env", "close_window", (int handle) =>
        {
            var window = NativeObjectManager.GetObject<PopupWindow>(memory.ReadInt32(handle));
            window.Close();
        });

        Linker.DefineFunction("env", "subscribe_to_event", (int controlId, int eventNameOffset, int eventNameLengthOffset) =>
        {
            string eventName = WasmMemoryUtility.GetString(eventNameOffset, eventNameLengthOffset);

            LayoutBuilder.ManagedElements[controlId].AddEvent(eventName, (args) =>
            {
                var action = Instance.GetAction<int, int>("raise_element_event");
                var ptr = WasmMemoryUtility.WriteString(eventName);

                action.Invoke(controlId, ptr);

                WasmMemoryUtility.Free(ptr);
            });
        });

        Linker.DefineFunction("env", "state_changed", (int controlId, int bodyOffset, int bodyLength) =>
        {
            Span<byte> arr = memory.GetSpan<byte>(bodyOffset, bodyLength);

            var element = LayoutBuilder.ManagedElements[controlId];
            var body = LayoutBuilder.Deserialize(arr, DuplicateResolutionTactic.ReplaceRemoveChildren);

            Dispatcher.UIThread.InvokeAsync(() =>
            {
                LayoutBuilder.ManagedElements[controlId] = element;
                if (element is StatefulContainer statefulElement && body is StatefulContainer statefulBodyElement)
                {
                    statefulElement.State.SetState(() => statefulElement.State.Content = statefulBodyElement.State.Content);
                }
            });
        });
    }
}
