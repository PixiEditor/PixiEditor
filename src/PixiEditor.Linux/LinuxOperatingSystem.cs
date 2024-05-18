using PixiEditor.OperatingSystem;

namespace PixiEditor.Linux;

public sealed class LinuxOperatingSystem : IOperatingSystem
{
    public string Name { get; } = "Linux";
    public IInputKeys InputKeys { get; }
    public IProcessUtility ProcessUtility { get; }
    
    public void OpenUri(string uri)
    {
        throw new NotImplementedException();
    }

    public void OpenFolder(string path)
    {
        throw new NotImplementedException();
    }
}
