namespace PixiEditor.Models.Commands.Templates;

internal interface IShortcutInstallation
{
    bool InstallationPresent { get; }

    ShortcutCollection GetInstalledShortcuts();
}
