using Wasmtime;

namespace PixiEditor.Extensions.WasmRuntime;

public class WasmExtensionInstance : Extension
{
    public Instance? Instance { get; private set; }

    private Linker Linker { get; }
    private Store Store { get; }
    private Module Module { get; }

    public WasmExtensionInstance(Linker linker, Store store, Module module)
    {
        Linker = linker;
        Store = store;
        Module = module;
    }

    public void Instantiate()
    {
        Linker.DefinePixiEditorApi(this);
        Linker.DefineModule(Store, Module);

        Instance = Linker.Instantiate(Store, Module);
    }

    protected override void OnInitialized()
    {
        Instance.GetAction("initialize").Invoke();
        base.OnInitialized();
    }

    protected override void OnLoaded()
    {
        Instance.GetFunction("_start").Invoke();
    }
}
