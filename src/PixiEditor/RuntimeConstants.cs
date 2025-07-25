using System.Diagnostics;

namespace PixiEditor;

public static class RuntimeConstants
{
    private static Dictionary<string, string> appSettings = File.Exists("appsettings.json")
        ? System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(ReadAppSettings())
        : new Dictionary<string, string>();

    private static string ReadAppSettings()
    {
        string installDirPath = Process.GetCurrentProcess().MainModule?.FileName;
        string appsettingsPath = Path.Combine(Path.GetDirectoryName(installDirPath) ?? string.Empty, "appsettings.json");
        using StreamReader reader = new StreamReader(appsettingsPath);
        return reader.ReadToEnd();
    }


    public static string? AnalyticsUrl =>
        appSettings.TryGetValue("AnalyticsUrl", out string? url) ? url : null;

    public static string? PixiEditorApiUrl =
        appSettings.TryGetValue("PixiEditorApiUrl", out string? apiUrl) ? apiUrl : null;

    public static string? PixiEditorApiKey =
        appSettings.TryGetValue("PixiEditorApiKey", out string? apiKey) ? apiKey : null;
}
