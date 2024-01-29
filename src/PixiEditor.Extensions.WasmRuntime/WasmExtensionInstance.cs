using System.Runtime.InteropServices;
using System.Text;
using Avalonia.Controls;
using Avalonia.Threading;
using PixiEditor.Extensions.CommonApi.LayoutBuilding;
using PixiEditor.Extensions.CommonApi.LayoutBuilding.State;
using PixiEditor.Extensions.LayoutBuilding.Elements;
using Wasmtime;

namespace PixiEditor.Extensions.WasmRuntime;

public class WasmExtensionInstance : Extension
{
    public Instance? Instance { get; private set; }

    private Linker Linker { get; }
    private Store Store { get; }
    private Module Module { get; }

    private Memory memory = null!;

    private Dictionary<int, ILayoutElement<Control>> managedElements = new();
    private LayoutBuilder LayoutBuilder { get; }
    private WasmMemoryUtility WasmMemoryUtility { get; set; }

    public WasmExtensionInstance(Linker linker, Store store, Module module)
    {
        Linker = linker;
        Store = store;
        Module = module;
        LayoutBuilder = new LayoutBuilder(managedElements);
    }

    public void Instantiate()
    {
        DefinePixiEditorApi();
        Linker.DefineModule(Store, Module);

        Instance = Linker.Instantiate(Store, Module);
        WasmMemoryUtility = new WasmMemoryUtility(Instance);
        Instance.GetFunction("_start").Invoke();
        memory = Instance.GetMemory("memory");
    }

    protected override void OnInitialized()
    {
        Instance.GetAction("initialize").Invoke();
        base.OnInitialized();
    }

    protected override void OnLoaded()
    {
        Instance.GetAction("load").Invoke();
        base.OnLoaded();
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

            Api.WindowProvider.CreatePopupWindow(title, body.BuildNative()).Show();
        });

        Linker.DefineFunction("env", "subscribe_to_event", (int controlId, int eventNameOffset, int eventNameLengthOffset) =>
        {
            string eventName = WasmMemoryUtility.GetString(eventNameOffset, eventNameLengthOffset);

            managedElements[controlId].AddEvent(eventName, (args) =>
            {
                var action = Instance.GetAction<int, int>("raise_element_event");
                var ptr = WasmMemoryUtility.WriteString(eventName);

                action.Invoke(controlId, ptr);
                //WasmMemoryUtility.Free(nameOffset);
            });
        });

        Linker.DefineFunction("env", "state_changed", (int controlId, int bodyOffset, int bodyLength) =>
        {
            Span<byte> arr = memory.GetSpan<byte>(bodyOffset, bodyLength);

            var element = managedElements[controlId];
            var body = LayoutBuilder.Deserialize(arr, DuplicateResolutionTactic.ReplaceRemoveChildren);

            Dispatcher.UIThread.InvokeAsync(() =>
            {
                managedElements[controlId] = element;
                if (element is StatefulContainer statefulElement && body is StatefulContainer statefulBodyElement)
                {
                    statefulElement.State.SetState(() => statefulElement.State.Content = statefulBodyElement.State.Content);
                }
            });
        });
    }
}
