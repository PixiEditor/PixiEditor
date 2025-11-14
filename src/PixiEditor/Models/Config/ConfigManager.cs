using System.Reflection;
using Avalonia.Platform;
using Newtonsoft.Json;
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
            catch (JsonReaderException)
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
        return JsonConvert.DeserializeObject<T>(json);
    }

    private T GetEmbeddedConfig<T>(string configName)
    {
        string path = Path.Combine(Paths.InternalResourceDataPath, $"Configs/{configName}.json");

        using Stream config = AssetLoader.Open(new Uri(path));
        using StreamReader reader = new(config);

        string json = reader.ReadToEnd();
        return JsonConvert.DeserializeObject<T>(json);
    }

    private void SaveConfig<T>(T config, string configName)
    {
        string path = Path.Combine(Paths.UserConfigPath, $"{configName}.json");
        string json = JsonConvert.SerializeObject(config, Formatting.Indented);

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
