﻿using System.IO;
using System.Reflection;

namespace PixiEditor.Models.IO;

public static class Paths
{
    public static string DataResourceUri { get; } = $"avares://{typeof(Paths).Assembly.GetName().Name}/Data/";

    public static string DataFullPath { get; } =
        Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Data");

    public static string InstallDirExtensionPackagesPath { get; } =
        Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Extensions");

    public static string LocalExtensionPackagesPath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "PixiEditor", "Extensions", "Packages");

    public static string UserConfigPath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "PixiEditor", "Configs");

    public static string UnpackedExtensionsPath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "PixiEditor", "Extensions", "Unpacked");

    public static string PathToPalettesFolder { get; } = Path.Join(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "PixiEditor", "Palettes");

    public static string PathToBrushesFolder { get; } = Path.Join(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "PixiEditor", "Brushes");

    public static string InternalResourceDataPath { get; } =
        $"avares://{Assembly.GetExecutingAssembly().GetName().Name}/Data";

    public static string TempRenderingPath { get; } = Path.Combine(Path.GetTempPath(), "PixiEditor", "Rendering");

    public static string TempFilesPath { get; } = Path.Combine(Path.GetTempPath(), "PixiEditor");
    public static string TempResourcesPath { get; } = Path.Combine(Path.GetTempPath(), "PixiEditor", "Resources");

    /// <summary>
    /// Path to %temp%/PixiEditor/Autosave
    /// </summary>
    public static string PathToUnsavedFilesFolder { get; } = Path.Join(
        Path.GetTempPath(),
        "PixiEditor", "Autosave");

    public static string InstallDirectoryPath { get; } =
        Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) ?? string.Empty;

    public static string ParseSpecialPathOrDefault(string path)
    {
        path = path.Replace("%appdata%", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
        path = path.Replace("%localappdata%",
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
        path = path.Replace("%temp%", Path.GetTempPath());

        return path;
    }
}
