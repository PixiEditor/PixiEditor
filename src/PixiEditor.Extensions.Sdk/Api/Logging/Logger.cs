using PixiEditor.Extensions.CommonApi.Logging;
using PixiEditor.Extensions.Sdk.Bridge;

namespace PixiEditor.Extensions.Sdk.Api.Logging;

public class Logger : ILogger
{
    public void Log(string message)
    {
        InvokeApiLog(message);
    }

    public void LogError(string message)
    {
        InvokeApiLog(message);
    }

    public void LogWarning(string message)
    {
        InvokeApiLog(message);
    }

    private void InvokeApiLog(string message)
    {
        Native.log_message(message);
    }
}
