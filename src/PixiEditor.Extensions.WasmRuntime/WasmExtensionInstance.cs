using System.Runtime.InteropServices;
using System.Text;
using Avalonia.Controls;
using Avalonia.Threading;
using PixiEditor.Extensions.Commands;
using PixiEditor.Extensions.CommonApi.Palettes;
using PixiEditor.Extensions.FlyUI;
using PixiEditor.Extensions.FlyUI.Elements;
using PixiEditor.Extensions.WasmRuntime.Api.Modules;
using PixiEditor.Extensions.WasmRuntime.Management;
using PixiEditor.Extensions.Windowing;
using Wasmtime;

namespace PixiEditor.Extensions.WasmRuntime;

public partial class WasmExtensionInstance : Extension
{
    public Instance? Instance { get; private set; }
    internal WasmMemoryUtility WasmMemoryUtility { get; set; }

    private Linker Linker { get; }
    private Store Store { get; }
    private Module Module { get; }

    private LayoutBuilder LayoutBuilder { get; set; }
    internal ObjectManager NativeObjectManager { get; set; }
    internal AsyncCallsManager AsyncHandleManager { get; set; }

    private WasmExtensionInstance Extension => this; // api group handler needs this property

    private string modulePath;
    private List<ApiModule> modules = new();

    public override string Location => modulePath;

    partial void LinkApiFunctions();

    public WasmExtensionInstance(Linker linker, Store store, Module module, string path)
    {
        Linker = linker;
        Store = store;
        Module = module;
        modulePath = path;
    }

    public void Instantiate()
    {
        NativeObjectManager = new ObjectManager();
        AsyncHandleManager = new AsyncCallsManager();
        AsyncHandleManager.OnAsyncCallCompleted += OnAsyncCallCompleted;
        AsyncHandleManager.OnAsyncCallFaulted += OnAsyncCallFaulted;

        LinkApiFunctions();
        Linker.DefineModule(Store, Module);

        Instance = Linker.Instantiate(Store, Module);
        WasmMemoryUtility = new WasmMemoryUtility(Instance);
    }

    protected override void OnLoaded()
    {
        Instance.GetAction("load").Invoke();
        base.OnLoaded();
    }

    protected override void OnInitialized()
    {
        modules.Add(new PreferencesModule(this, Api.Preferences));
        modules.Add(new CommandModule(this, Api.Commands,
            (ICommandSupervisor)Api.Services.GetService(typeof(ICommandSupervisor))));
        LayoutBuilder = new LayoutBuilder((ElementMap)Api.Services.GetService(typeof(ElementMap)));

        //SetElementMap();
        Instance.GetAction("initialize").Invoke();
        base.OnInitialized();
    }

    private void OnAsyncCallCompleted(int handle, int result)
    {
        Dispatcher.UIThread.Invoke(() =>
            Instance.GetAction<int, int>("async_call_completed").Invoke(handle, result));
    }

    private void OnAsyncCallFaulted(int handle, string exceptionMessage)
    {
        Dispatcher.UIThread.Invoke(() =>
            Instance.GetAction<int, string>("async_call_faulted").Invoke(handle, exceptionMessage));
    }

    private void SetElementMap()
    {
        var elementMap = (ElementMap)Api.Services.GetService(typeof(ElementMap));
        byte[] map = elementMap.Serialize();
        var ptr = WasmMemoryUtility.WriteBytes(map);
        Instance.GetAction<int, int>("set_element_map").Invoke(ptr, map.Length);

        WasmMemoryUtility.Free(ptr);
    }

    public T? GetModule<T>() where T : ApiModule
    {
        var module = modules.FirstOrDefault(x => x.GetType() == typeof(T));
        if (module == null)
        {
            return default;
        }

        return (T)module;
    }
}
