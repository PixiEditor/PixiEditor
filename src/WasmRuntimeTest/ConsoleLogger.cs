using PixiEditor.Extensions.Wasm.Api;

namespace WasmRuntimeTest;

public class ConsoleLogger : ILogger
{
    public void Log(string message)
    {
        Console.WriteLine(message);
    }

    public void LogError(string message)
    {
        Log(message);
    }

    public void LogWarning(string message)
    {
        Log(message);
    }
}
