using System.IO;
using System.Text;
using System.Xml;

namespace PixiEditor.Models.Commands.Templates.Parsers;

/// <summary>
///     Aseprite uses XML (under .aseprite-keys file) to store keybindings.
/// This class is used to load and parse this file into <see cref="ShortcutsTemplate"/> class.
///
/// Aseprite keys consists of 5 sections:
/// <list type="">
///     <item>Commands</item>
///     <item>Tools</item>
///     <item>QuickTools</item>
///     <item>Actions</item>
///     <item>Actions</item>
/// </list>
///  
/// We are only interested in Commands and Tools sections, because actions (like binding Left Mouse Button to some shortcut)
/// are not yet supported by us.
/// </summary>
public class AsepriteKeysParser : KeysParser
{
    public AsepriteKeysParser(string mapFileName) : base(mapFileName)
    {
    }

    public override ShortcutsTemplate Parse(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("File not found", path);
        }

        if (Path.GetExtension(path) != ".aseprite-keys")
        {
            throw new ArgumentException("File is not aseprite-keys file", nameof(path));
        }
        
        return LoadAndParse(path);
    }

    private ShortcutsTemplate LoadAndParse(string path)
    {
        XmlDocument doc = new XmlDocument();
        doc.Load(path);
        
        List<KeyDefinition> keyDefinitions = new List<KeyDefinition>(); // DefaultShortcut is actually mapped shortcut.

        LoadCommands(doc, keyDefinitions);
        LoadTools(doc, keyDefinitions);
        
        ShortcutsTemplate template = ShortcutsTemplate.FromKeyDefinitions(keyDefinitions);
        return template;
    }

    private void LoadCommands(XmlDocument document, List<KeyDefinition> keyDefinitions)
    {
        XmlNodeList commands = document.SelectNodes("keyboard/commands/key");
        ApplyDefaults(keyDefinitions, "PixiEditor");


        foreach (XmlNode commandNode in commands)
        {
            if(commandNode.Attributes == null) continue;
            
            XmlAttribute command = commandNode.Attributes["command"];
            XmlAttribute shortcut = commandNode.Attributes["shortcut"];
            XmlNodeList paramNodes = commandNode.SelectNodes("param");

            if(command == null || shortcut == null) continue;

            string commandName = $"{command.Value}{GetParamString(paramNodes)}";
            string shortcutValue = shortcut.Value;
            
            if (!Map.ContainsKey(commandName))
            {
                continue;
            }

            var mappedEntry = Map[commandName];
            commandName = mappedEntry.Command;
            
            HumanReadableKeyCombination combination;
            
            XmlAttribute removed = commandNode.Attributes["removed"];
            if (removed is { Value: "true" })
            {
                combination = new HumanReadableKeyCombination("None");
            }
            else
            {
                combination = HumanReadableKeyCombination.FromStringCombination(shortcutValue);
            }

            // We should override existing entry, because aseprite-keys file can contain multiple entries for the same command.
            // Last one is the one that should be used.
            keyDefinitions.RemoveAll(x => x.Command == commandName);
            
            keyDefinitions.Add(new KeyDefinition(commandName, combination));
        }
    }

    private string GetParamString(XmlNodeList paramNodes)
    {
        if(paramNodes == null || paramNodes.Count == 0) return string.Empty;
        
        StringBuilder builder = new StringBuilder();
        foreach (XmlNode paramNode in paramNodes)
        {
            if(paramNode.Attributes == null) continue;
            
            XmlAttribute paramName = paramNode.Attributes["name"];
            XmlAttribute paramValue = paramNode.Attributes["value"];
            if(paramName == null || paramValue == null) continue;

            builder.Append('.');
            builder.Append(paramName.Value);
            builder.Append('=');
            builder.Append(paramValue.Value);
        }

        return builder.ToString();
    }

    /// <summary>
    ///     Tools are stored in keyboard > tools section of aseprite-keys file.
    /// Each tool entry is a key XML node with command attribute under 'tool' parameter and shortcut under 'shortcut' parameter.
    /// </summary>
    /// <param name="keyDefinitions">Definitions to write to</param>
    private void LoadTools(XmlDocument document, List<KeyDefinition> keyDefinitions)
    {
        XmlNodeList tools = document.SelectNodes("keyboard/tools/key");
        ApplyDefaults(keyDefinitions, "PixiEditor.Tools");

        foreach (XmlNode tool in tools)
        {
            if(tool.Attributes == null) continue;
            
            XmlAttribute command = tool.Attributes["tool"];
            XmlAttribute shortcut = tool.Attributes["shortcut"];

            if(command == null || shortcut == null) continue;

            string commandName = command.Value;
            string shortcutValue = shortcut.Value;

            if (!Map.ContainsKey(commandName))
            {
                continue;
            }

            var mappedEntry = Map[commandName];
            commandName = mappedEntry.Command;

            HumanReadableKeyCombination combination;
            
            XmlAttribute removed = tool.Attributes["removed"];
            if (removed is { Value: "true" })
            {
                combination = new HumanReadableKeyCombination("None");
            }
            else
            {
                combination = HumanReadableKeyCombination.FromStringCombination(shortcutValue);
            }

            // We should override existing entry, because aseprite-keys file can contain multiple entries for the same tool.
            // Last one is the one that should be used.
            keyDefinitions.RemoveAll(x => x.Command == commandName);
            
            keyDefinitions.Add(new KeyDefinition(commandName, combination));
        }
    }

    private void ApplyDefaults(List<KeyDefinition> keyDefinitions, string commandGroup)
    {
        foreach (var mapEntry in Map)
        {
            if (mapEntry.Value.Command.StartsWith(commandGroup))
            {
                keyDefinitions.Add(mapEntry.Value);
            }
        }
    }
}
