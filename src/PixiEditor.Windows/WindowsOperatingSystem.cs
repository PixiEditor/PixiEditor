using PixiEditor.OperatingSystem;

namespace PixiEditor.Windows;

public class WindowsOperatingSystem : IOperatingSystem
{
    public string Name => "Windows";
    public IInputKeys InputKeys { get; } = new WindowsInputKeys();

    public WindowsOperatingSystem() => IOperatingSystem.SetCurrent(this);
}
