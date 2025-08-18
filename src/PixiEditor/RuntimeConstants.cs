using System.Diagnostics;
using System.Reflection;
using PixiEditor.Models.IO;

namespace PixiEditor;

public static class RuntimeConstants
{
    private static Dictionary<string, string> appSettings =
        System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(ReadAppSettings());

    private static string ReadAppSettings()
    {
        string installDirPath = Paths.InstallDirectoryPath;
        string appsettingsPath = Path.Combine(installDirPath, "appsettings.json");
        if (!File.Exists(appsettingsPath))
        {
            return "{}";
        }
        
        using StreamReader reader = new StreamReader(appsettingsPath);
        return reader.ReadToEnd();
    }


    public static string? AnalyticsUrl =>
        appSettings.TryGetValue("AnalyticsUrl", out string? url) ? url : null;

    public static string? PixiEditorApiUrl =>
        appSettings.TryGetValue("PixiEditorApiUrl", out string? apiUrl) ? apiUrl : null;

    public static string? PixiEditorApiKey =>
        appSettings.TryGetValue("PixiEditorApiKey", out string? apiKey) ? apiKey : null;
}
