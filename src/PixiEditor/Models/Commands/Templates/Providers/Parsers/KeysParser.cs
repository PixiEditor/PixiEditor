using System.IO;
using Newtonsoft.Json;

namespace PixiEditor.Models.Commands.Templates.Parsers;

public abstract class KeysParser
{
    public string MapFileName { get; }

    private static string _fullMapFilePath;

    public Dictionary<string, KeyDefinition> Map => _cachedMap ??= LoadKeysMap();
    private Dictionary<string, KeyDefinition> _cachedMap;
    
    public List<Shortcut> Defaults => _cachedDefaults ??= ParseDefaults();
    private List<Shortcut> _cachedDefaults;
    
    public KeysParser(string mapFileName)
    {
        _fullMapFilePath = Path.Combine("Data", "ShortcutActionMaps", mapFileName);
        if (!File.Exists(_fullMapFilePath))
        {
            throw new FileNotFoundException($"Keys map file '{_fullMapFilePath}' not found.");
        }
        
        MapFileName = mapFileName;
    }
    
    public abstract ShortcutsTemplate Parse(string filePath);
    
    private Dictionary<string, KeyDefinition> LoadKeysMap()
    {
        string text = File.ReadAllText(_fullMapFilePath);
        var dict = JsonConvert.DeserializeObject<Dictionary<string, KeyDefinition>>(text);
        if(dict == null) throw new Exception("Keys map file is empty.");
        if(dict.ContainsKey("")) dict.Remove("");
        return dict;
    }
    
    private List<Shortcut> ParseDefaults()
    {
        var defaults = new List<Shortcut>();
        foreach (var (key, value) in Map)
        {
            if (value.DefaultShortcut != null)
            {
                defaults.Add(new Shortcut(value.DefaultShortcut.ToKeyCombination(), Map[key].Command));
            }
        }
        
        return defaults;
    }
}
