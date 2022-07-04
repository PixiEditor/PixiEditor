namespace PixiEditor.Models.Commands.Templates;

public interface IShortcutInstallation
{
    bool InstallationPresent { get; }
    
    ShortcutCollection GetInstalledShortcuts();
}