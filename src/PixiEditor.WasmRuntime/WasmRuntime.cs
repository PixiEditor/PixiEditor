using System.Runtime.InteropServices;
using System.Text;
using Wasmtime;
using IntPtr = System.IntPtr;

namespace PixiEditor.WasmRuntime;

public class WasmRuntime
{
    private Engine engine = new Engine();

    public unsafe void LoadModule(string path)
    {
        Module module = Module.FromFile(engine, path);
        WasiConfiguration wasiConfig = new WasiConfiguration();
        wasiConfig.WithInheritedStandardError().WithInheritedStandardInput().WithInheritedStandardOutput()
            .WithInheritedArgs().WithInheritedEnvironment();

        using var config = new Config().WithDebugInfo(true)
            .WithCraneliftDebugVerifier(true)
            .WithOptimizationLevel(OptimizationLevel.SpeedAndSize)
            .WithWasmThreads(true)
            .WithBulkMemory(true)
            .WithMultiMemory(true);

        var linker = new Linker(engine);
        var store = new Store(engine);
        store.SetWasiConfiguration(wasiConfig);
        linker.DefineWasi();

        Instance? instance = null;

        linker.DefineFunction("env", "log_message",(int message) =>
        {
            string messageString = GetFromWasmMemory(message, instance.GetMemory("memory"));
            Console.WriteLine(messageString);
        });

        linker.DefineModule(store, module);

        instance = linker.Instantiate(store, module);

        instance.GetFunction("_start").Invoke();
    }

    private string GetFromWasmMemory(int offset, Memory memory)
    {
        var span = memory.GetSpan<byte>(offset);
        int length = 0;
        while (span[length] != 0)
        {
            length++;
        }

        var buffer = new byte[length];
        span.Slice(0, length).CopyTo(buffer);
        return Encoding.UTF8.GetString(buffer);
    }
}
