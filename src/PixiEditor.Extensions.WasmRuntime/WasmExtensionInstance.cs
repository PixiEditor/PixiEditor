using Wasmtime;

namespace PixiEditor.Extensions.WasmRuntime;

public class WasmExtensionInstance : Extension
{
    public Instance? Instance { get; private set; }

    private Linker Linker { get; }
    private Store Store { get; }
    private Module Module { get; }

    private Memory memory = null!;

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
        Instance.GetFunction("_start").Invoke();
        memory = Instance.GetMemory("memory");
    }

    protected override void OnInitialized()
    {
        Instance.GetAction("initialize").Invoke();
        int testId = 69;
        int ptr = MemoryUtility.WriteInt32(Instance, memory, testId);
        int pt2 = MemoryUtility.WriteString(Instance, memory, "Test event");

        Instance.GetAction<int, int>("raise_element_event").Invoke(ptr, pt2);

        base.OnInitialized();
    }

    protected override void OnLoaded()
    {
        Instance.GetAction("load").Invoke();
        base.OnLoaded();
    }
}
