namespace PixiEditor.Extensions.CommonApi.Logging;

public interface ILogger
{
    public void Log(string message);
    public void LogError(string message);
    public void LogWarning(string message);
}
