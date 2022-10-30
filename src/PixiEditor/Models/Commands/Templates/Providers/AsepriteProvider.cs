using System.IO;
using System.Windows.Input;
using PixiEditor.Models.Commands.Templates.Parsers;

namespace PixiEditor.Models.Commands.Templates;

internal partial class ShortcutProvider
{
    public static AsepriteProvider Aseprite { get; } = new();

    internal class AsepriteProvider : ShortcutProvider, IShortcutDefaults, IShortcutInstallation
    {
        private static string InstallationPath { get; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Aseprite", "user.aseprite-keys");

        private AsepriteKeysParser _parser;
        
        public AsepriteProvider() : base("Aseprite")
        {
            _parser = new AsepriteKeysParser("AsepriteShortcutMap.json");
            LogoPath = "/Images/TemplateLogos/Aseprite.png";
            HoverLogoPath = "/Images/TemplateLogos/Aseprite-Hover.png";
        }

        public bool InstallationPresent => File.Exists(InstallationPath);
        
        public ShortcutsTemplate GetInstalledShortcuts()
        {
            return _parser.Parse(InstallationPath);
        }

        public List<Shortcut> DefaultShortcuts => _parser.Defaults;
    }
}
