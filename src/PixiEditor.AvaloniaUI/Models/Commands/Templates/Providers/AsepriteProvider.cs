﻿using System.Collections.Generic;
using System.IO;
using PixiEditor.AvaloniaUI.Models.Commands.Templates.Providers.Parsers;

namespace PixiEditor.AvaloniaUI.Models.Commands.Templates.Providers;

internal partial class ShortcutProvider
{
    public static AsepriteProvider Aseprite { get; } = new();

    internal class AsepriteProvider : Templates.ShortcutProvider, IShortcutDefaults, IShortcutInstallation, ICustomShortcutFormat
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
            return _parser.Parse(InstallationPath, true);
        }

        public List<Shortcut> DefaultShortcuts => _parser.Defaults;
        public KeysParser KeysParser => _parser;
        public string[] CustomShortcutExtensions => new[] { ".aseprite-keys" };
    }
}