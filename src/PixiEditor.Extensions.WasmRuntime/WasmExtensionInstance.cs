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

public partial class WasmExtensionInstance : Extension
{
    public Instance? Instance { get; private set; }

    private Linker Linker { get; }
    private Store Store { get; }
    private Module Module { get; }

    private Memory memory = null!;
    private LayoutBuilder LayoutBuilder { get; set; }
    private ObjectManager NativeObjectManager { get; set; }
    private AsyncCallsManager AsyncHandleManager { get; set; }
    private WasmMemoryUtility WasmMemoryUtility { get; set; }

    partial void LinkApiFunctions();

    public WasmExtensionInstance(Linker linker, Store store, Module module)
    {
        Linker = linker;
        Store = store;
        Module = module;
    }

    public void Instantiate()
    {
        NativeObjectManager = new ObjectManager();
        AsyncHandleManager = new AsyncCallsManager();
        AsyncHandleManager.OnAsyncCallCompleted += OnAsyncCallCompleted;
        AsyncHandleManager.OnAsyncCallFaulted += AsyncHandleManagerOnOnAsyncCallFaulted;
        
        LinkApiFunctions();
        Linker.DefineModule(Store, Module);

        Instance = Linker.Instantiate(Store, Module);
        WasmMemoryUtility = new WasmMemoryUtility(Instance);
        memory = Instance.GetMemory("memory");
    }

    private void OnAsyncCallCompleted(int handle, int result)
    {
        Instance.GetAction<int, int>("async_call_completed").Invoke(handle, result);
    }
    
    private void AsyncHandleManagerOnOnAsyncCallFaulted(int handle, string exceptionMessage)
    {
        Instance.GetAction<int, string>("async_call_faulted").Invoke(handle, exceptionMessage);
    }

    protected override void OnLoaded()
    {
        Instance.GetAction("load").Invoke();
        base.OnLoaded();
    }

    protected override void OnInitialized()
    {
        LayoutBuilder = new LayoutBuilder((ElementMap)Api.Services.GetService(typeof(ElementMap)));

        //SetElementMap();
        Instance.GetAction("initialize").Invoke();
        base.OnInitialized();
    }

    private void SetElementMap()
    {
        var elementMap = (ElementMap)Api.Services.GetService(typeof(ElementMap));
        byte[] map = elementMap.Serialize();
        var ptr = WasmMemoryUtility.WriteBytes(map);
        Instance.GetAction<int, int>("set_element_map").Invoke(ptr, map.Length);

        WasmMemoryUtility.Free(ptr);
    }
}
