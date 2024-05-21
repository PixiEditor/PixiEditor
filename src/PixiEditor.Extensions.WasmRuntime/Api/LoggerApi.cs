namespace PixiEditor.Extensions.WasmRuntime.Api;

internal class LoggerApi : ApiGroupHandler
{
    [ApiFunction("log_message")]
    public void Log(string message)
    {
        Console.WriteLine(message.ReplaceLineEndings());
    }
}
