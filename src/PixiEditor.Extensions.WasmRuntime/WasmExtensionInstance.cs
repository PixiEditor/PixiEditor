using System.Runtime.InteropServices;
using System.Text;
using Avalonia.Controls;
using Avalonia.Threading;
using PixiEditor.Extensions.CommonApi.LayoutBuilding;
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

    private Action<int, int> raiseElementEvent;

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
        raiseElementEvent = Instance.GetAction<int, int>("raise_element_event");
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

            var body = LayoutBuilder.Deserialize(arr);

            Api.WindowProvider.CreatePopupWindow(title, body.Build()).ShowDialog();
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
    }
}
