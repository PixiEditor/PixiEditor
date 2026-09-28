namespace PixiEditor.Extensions.Runtime;

public interface IHost
{
    public string HostName { get; }
    public Version Version { get; }
}
