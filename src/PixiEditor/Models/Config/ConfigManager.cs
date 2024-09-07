using System.Reflection;
using Avalonia.Platform;
using Newtonsoft.Json;
using PixiEditor.Views;

namespace PixiEditor.Models.Config;

public class ConfigManager
{
    public T GetConfig<T>(string configName)
    {
        string path = $"avares://{Assembly.GetExecutingAssembly().GetName().Name}/Data/Configs/{configName}.json";

        using Stream config = AssetLoader.Open(new Uri(path));
        using StreamReader reader = new(config);
        
        string json = reader.ReadToEnd();
        return JsonConvert.DeserializeObject<T>(json);
    }
}
