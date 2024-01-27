using System.Text;
using Wasmtime;

namespace PixiEditor.Extensions.WasmRuntime;

public class WasmRuntime
{
    private Engine engine = new Engine();

    public WasmExtensionInstance LoadModule(string path)
    {
        if (!System.IO.File.Exists(path))
        {
            throw new System.IO.FileNotFoundException("File not found", path);
        }

        Module module = Module.FromFile(engine, path);
        WasiConfiguration wasiConfig = new WasiConfiguration();
        wasiConfig.WithInheritedStandardError().WithInheritedStandardInput().WithInheritedStandardOutput()
            .WithInheritedArgs().WithInheritedEnvironment();

        using var config = new Config().WithDebugInfo(true)
            .WithCraneliftDebugVerifier(true)
            .WithReferenceTypes(true)
            .WithOptimizationLevel(OptimizationLevel.SpeedAndSize)
            .WithWasmThreads(true)
            .WithBulkMemory(true)
            .WithMultiMemory(true);

        var linker = new Linker(engine);
        var store = new Store(engine);
        store.SetWasiConfiguration(wasiConfig);
        linker.DefineWasi();
        return new WasmExtensionInstance(linker, store, module);
    }
}
