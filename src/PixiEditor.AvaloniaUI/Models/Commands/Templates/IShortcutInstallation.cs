namespace PixiEditor.AvaloniaUI.Models.Commands.Templates;

internal interface IShortcutInstallation
{
    bool InstallationPresent { get; }

    ShortcutsTemplate GetInstalledShortcuts();
}
