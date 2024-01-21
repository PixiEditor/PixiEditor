using Wasmtime;

namespace PixiEditor.WasmRuntime;

public class WasmRuntime
{
    private Engine engine = new Engine();

    public void LoadModule(string path)
    {
        Module module = Module.FromFile(engine, path);
        WasiConfiguration wasiConfig = new WasiConfiguration();

        var linker = new Linker(engine);
        var store = new Store(engine);
        store.SetWasiConfiguration(wasiConfig);

        linker.DefineWasi();

        var instance = linker.Instantiate(store, module);

        instance.GetAction("ProvideApi").Invoke();
        var main = instance.GetAction("Entry");

        main.Invoke();
    }
}
