using System.Reflection;
using Avalonia.Platform;
using Newtonsoft.Json;
using PixiEditor.Models.IO;
using PixiEditor.Views;

namespace PixiEditor.Models.Config;

public class ConfigManager
{
    public T GetConfig<T>(string configName)
    {
        // TODO: Local configs require a mechanism that will allow to update them when the embedded config changes
        // but merges the changes with the local config or something like that, leaving as is for now
        /*if (LocalConfigExists(configName))
        {
            try
            {
                return GetLocalConfig<T>(configName);
            }
            catch(JsonReaderException)
            {
                // If the local config is corrupted, delete it and load the embedded one
                File.Delete(Path.Combine(Paths.UserConfigPath, $"Configs/{configName}.json"));
            }
        }*/

        var embeddedConfig = GetEmbeddedConfig<T>(configName);
        //SaveConfig(embeddedConfig, configName);
        return embeddedConfig;
    }

    private T GetLocalConfig<T>(string configName)
    {
        string path = $"Configs/{configName}.json";
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
        string path = Path.Combine(Paths.UserConfigPath, $"Configs/{configName}.json");
        string json = JsonConvert.SerializeObject(config, Formatting.Indented);

        Directory.CreateDirectory(Path.GetDirectoryName(path));
        using FileStream file = File.Open(path, FileMode.Create);
        using StreamWriter writer = new(file);

        writer.Write(json);
    }
    
    private bool LocalConfigExists(string configName)
    {
        string path = Path.Combine(Paths.UserConfigPath, $"Configs/{configName}.json");
        return File.Exists(path);
    }
}
