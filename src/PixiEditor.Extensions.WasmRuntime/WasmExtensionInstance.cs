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
using PixiEditor.Extensions.WasmRuntime.Utilities;
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

    internal LayoutBuilder LayoutBuilder { get; set; }
    internal ObjectManager NativeObjectManager { get; set; }
    internal AsyncCallsManager AsyncHandleManager { get; set; }

    private WasmExtensionInstance Extension => this; // api group handler needs this property

    private string modulePath;
    private List<ApiModule> modules = new();

    public override string Location => modulePath;
    public bool HasEncryptedResources
    {
        get
        {
            var key = GetEncryptionKey();
            var iv = GetEncryptionIV();
            if (key.Length == 0 || iv.Length == 0)
            {
                return false;
            }

            if(key.All(x => x == 0) || iv.All(x => x == 0))
            {
                return false;
            }

            return true;
        }
    }

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

    protected override int GetApiVersion()
    {
        int version = Instance.GetFunction("get_api_version")?.Invoke() as int? ?? 1;
        return version;
    }

    protected override void OnLoaded()
    {
        Instance.GetAction("load")?.Invoke();
        base.OnLoaded();
    }

    protected override void OnInitialized()
    {
        modules.Add(new WindowingModule(this));
        modules.Add(new UiModule(this));
        modules.Add(new PreferencesModule(this, Api.Preferences));
        modules.Add(new CommandModule(this, Api.Commands,
            (ICommandSupervisor)Api.Services.GetService(typeof(ICommandSupervisor))));
        modules.Add(new EventsModule(this));
        modules.Add(new ExtensionsModule(this));
        LayoutBuilder = new LayoutBuilder(new ExtensionResourceStorage(this), (ElementMap)Api.Services.GetService(typeof(ElementMap)));
        //SetElementMap();
        try
        {
            Instance.GetAction("initialize")?.Invoke();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception during extension initialization: " + ex);
        }

        base.OnInitialized();
    }

    protected override void OnUserReady()
    {
        Instance.GetAction("user_ready")?.Invoke();
        base.OnUserReady();
    }

    protected override void OnMainWindowLoaded()
    {
        Instance.GetAction("main_window_loaded")?.Invoke();
        base.OnMainWindowLoaded();
    }

    public byte[] GetEncryptionKey()
    {
        int ptr = Instance.GetFunction("get_encryption_key")?.Invoke() as int? ?? 0;
        if (ptr == 0)
        {
            return [];
        }

        return WasmMemoryUtility.GetBytes(ptr, 16);
    }

    public byte[] GetEncryptionIV()
    {
        int ptr = Instance.GetFunction("get_encryption_iv")?.Invoke() as int? ?? 0;
        if (ptr == 0)
        {
            return [];
        }

        return WasmMemoryUtility.GetBytes(ptr, 16);
    }

    private void OnAsyncCallCompleted(int handle, int result)
    {
        Dispatcher.UIThread.Invoke(() =>
            Instance.GetAction<int, int>("async_call_completed")?.Invoke(handle, result));
    }

    private void OnAsyncCallFaulted(int handle, string exceptionMessage)
    {
        Dispatcher.UIThread.Invoke(() =>
            Instance.GetAction<int, string>("async_call_faulted")?.Invoke(handle, exceptionMessage));
    }

    private void SetElementMap()
    {
        var elementMap = (ElementMap)Api.Services.GetService(typeof(ElementMap));
        byte[] map = elementMap.Serialize();
        var ptr = WasmMemoryUtility.WriteBytes(map);
        Instance.GetAction<int, int>("set_element_map")?.Invoke(ptr, map.Length);

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
