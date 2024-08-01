namespace PixiEditor.Models.Commands.Templates;

internal partial class ShortcutProvider
{
    public string Name { get; set; }
    
    public string LogoPath { get; set; }
    public string HoverLogoPath { get; set; }

    /// <summary>
    /// Set this to true if this provider has default shortcuts
    /// </summary>
    public bool HasDefaultShortcuts => this is IShortcutDefaults;

    /// <summary>
    /// Set this to true if this provider can provide from a file
    /// </summary>
    public bool ProvidesImport => this is IShortcutFile;

    /// <summary>
    /// Set this to true if this provider can provide from installation
    /// </summary>
    public bool ProvidesFromInstallation => this is IShortcutInstallation;

    public bool HasInstallationPresent => (this as IShortcutInstallation)?.InstallationPresent ?? false;

    public virtual string Description { get; } = string.Empty;

    public ShortcutProvider(string name)
    {
        Name = name;
    }

    public static ShortcutProvider[] GetProviders() => new ShortcutProvider[]
    {
        #if DEBUG
        Providers.ShortcutProvider.Debug,
        #endif
        Providers.ShortcutProvider.Aseprite
    };
}
