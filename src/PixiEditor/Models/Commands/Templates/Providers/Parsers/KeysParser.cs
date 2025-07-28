using System.Collections.Generic;
using System.IO;
using Avalonia.Platform;
using Newtonsoft.Json;
using PixiEditor.Models.IO;

namespace PixiEditor.Models.Commands.Templates.Providers.Parsers;

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
        if (mapFileName.StartsWith("avares://"))
        {
            SetResourcePathOrThrow(mapFileName);
        }
        else
        {
            SetPathOrThrow(mapFileName);
        }

        MapFileName = mapFileName;
    }

    private void SetResourcePathOrThrow(string mapFileName)
    {
        _fullMapFilePath = mapFileName;
        if (!AssetLoader.Exists(new Uri(mapFileName)))
        {
            throw new FileNotFoundException($"Keys map file '{_fullMapFilePath}' not found in resources.");
        }
    }

    private void SetPathOrThrow(string mapFileName)
    {
        _fullMapFilePath = Path.Combine(Paths.DataFullPath, "ShortcutActionMaps", mapFileName);
        if (!File.Exists(_fullMapFilePath))
        {
            throw new FileNotFoundException($"Keys map file '{_fullMapFilePath}' not found.");
        }
    }

    /// <summary>
    ///     Parses custom shortcuts file into ShortcutTemplate.
    /// </summary>
    /// <param name="filePath">Path to the file.</param>
    /// <param name="applyTemplateDefaults">If true, all shortcuts available in the key map will be loaded, and then overwritten by entries in the file. If false, only entries from the file will be applied.</param>
    /// <returns>Parsed ShortcutTemplate.</returns>
    public abstract ShortcutsTemplate Parse(string filePath, bool applyTemplateDefaults);

    private Dictionary<string, KeyDefinition> LoadKeysMap()
    {
        string text = ReadMap();
        var dict = JsonConvert.DeserializeObject<Dictionary<string, KeyDefinition>>(text);
        if (dict == null) throw new Exception("Keys map file is empty.");
        if (dict.ContainsKey("")) dict.Remove("");
        return dict;
    }

    private static string ReadMap()
    {
        if (_fullMapFilePath.StartsWith("avares://"))
        {
            using Stream stream = AssetLoader.Open(new Uri(_fullMapFilePath));
            using StreamReader reader = new(stream);
            return reader.ReadToEnd();
        }

        return File.ReadAllText(_fullMapFilePath);
    }

    private List<Shortcut> ParseDefaults()
    {
        var defaults = new List<Shortcut>();
        foreach (var (key, value) in Map)
        {
            if (value.DefaultShortcut != null)
            {
                foreach (var keyCommand in Map[key].Commands)
                {
                    defaults.Add(new Shortcut(value.DefaultShortcut.ToKeyCombination(), keyCommand));
                }
            }
        }

        return defaults;
    }
}
