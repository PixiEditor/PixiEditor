using System.Reflection;
using System.Text.Json;
using Avalonia.Platform;
using PixiEditor.Helpers;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.IO;
using PixiEditor.Views;

namespace PixiEditor.Models.Config;

public class ConfigManager
{
    public T GetConfig<T>(string configName) where T : IMergeable<T>
    {
        var embeddedConfig = GetEmbeddedConfig<T>(configName);
        if (LocalConfigExists(configName))
        {
            try
            {
                return embeddedConfig.TryMergeWith(GetLocalConfig<T>(configName));
            }
            catch (JsonException)
            {
                return embeddedConfig;
            }
        }

        return embeddedConfig;
    }

    private T GetLocalConfig<T>(string configName)
    {
        string path = $"{configName}.json";
        using FileStream file = File.Open(Path.Combine(Paths.UserConfigPath, path), FileMode.Open);
        using StreamReader reader = new(file);

        string json = reader.ReadToEnd();
        return JsonSerializer.Deserialize<T>(json, JsonOptions.CasesInsensitive);
    }

    private T GetEmbeddedConfig<T>(string configName)
    {
        string path = Path.Combine(Paths.InternalResourceDataPath, $"Configs/{configName}.json");

        using Stream config = AssetLoader.Open(new Uri(path));
        using StreamReader reader = new(config);

        string json = reader.ReadToEnd();
        return JsonSerializer.Deserialize<T>(json, JsonOptions.CasesInsensitive);
    }

    private void SaveConfig<T>(T config, string configName)
    {
        string path = Path.Combine(Paths.UserConfigPath, $"{configName}.json");
        string json = JsonSerializer.Serialize(config, JsonOptions.CasesInsensitiveIndented);

        Directory.CreateDirectory(Path.GetDirectoryName(path));
        using FileStream file = File.Open(path, FileMode.Create);
        using StreamWriter writer = new(file);

        writer.Write(json);
    }

    private bool LocalConfigExists(string configName)
    {
        string path = Path.Combine(Paths.UserConfigPath, $"{configName}.json");
        return File.Exists(path);
    }
}
