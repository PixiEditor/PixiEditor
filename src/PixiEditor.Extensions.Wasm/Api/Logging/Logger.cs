﻿namespace PixiEditor.Extensions.Wasm.Api.Logging;

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
        Interop.LogMessage(message);
    }
}