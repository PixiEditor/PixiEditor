using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using PixiEditor.Exceptions;
using PixiEditor.Extensions.Exceptions;

namespace PixiEditor.Models.Commands.Templates.Providers.Parsers;

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

    public override ShortcutsTemplate Parse(string path, bool applyDefaults)
    {
        if (!File.Exists(path))
        {
            throw new MissingFileException("FILE_NOT_FOUND", $"File {path} not found");
        }

        if (Path.GetExtension(path) != ".aseprite-keys")
        {
            throw new InvalidFileTypeException("FILE_FORMAT_NOT_ASEPRITE_KEYS",
                $"File {path} is not an aseprite-keys file");
        }

        return LoadAndParse(path, applyDefaults);
    }

    private ShortcutsTemplate LoadAndParse(string path, bool applyDefaults)
    {
        XmlDocument doc = new XmlDocument();

        try
        {
            doc.Load(path);
        }
        catch (Exception e) when (e is DirectoryNotFoundException or FileNotFoundException or PathTooLongException)
        {
            throw new MissingFileException("FILE_NOT_FOUND", e);
        }
        catch (Exception e)
        {
            throw new RecoverableException("FAILED_TO_OPEN_FILE", e);
        }

        List<KeyDefinition> keyDefinitions = new List<KeyDefinition>(); // DefaultShortcut is actually mapped shortcut.

        LoadCommands(doc, keyDefinitions, applyDefaults);
        LoadTools(doc, keyDefinitions, applyDefaults);

        try
        {
            return ShortcutsTemplate.FromKeyDefinitions(keyDefinitions);
        }
        catch (RecoverableException) { throw; }
        catch (Exception e)
        {
            throw new CorruptedFileException("FILE_HAS_INVALID_SHORTCUT", e);
        }
    }

    private void LoadCommands(XmlDocument document, List<KeyDefinition> keyDefinitions, bool applyDefaults)
    {
        XmlNodeList commands = document.SelectNodes("keyboard/commands/key");
        if (applyDefaults)
        {
            ApplyDefaults(keyDefinitions, "PixiEditor");
        }

        foreach (XmlNode commandNode in commands)
        {
            if (commandNode.Attributes == null) continue;

            XmlAttribute command = commandNode.Attributes["command"];
            XmlAttribute shortcut = commandNode.Attributes["shortcut"];
            XmlNodeList paramNodes = commandNode.SelectNodes("param");

            if (command == null || shortcut == null) continue;

            string commandName = $"{command.Value}{GetParamString(paramNodes)}";
            string shortcutValue = shortcut.Value;

            if (!Map.ContainsKey(commandName))
            {
                continue;
            }

            var mappedEntry = Map[commandName];
            foreach (var mappedCommand in mappedEntry.Commands)
            {
                commandName = mappedCommand;

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
                keyDefinitions.RemoveAll(x => x.Commands.Contains(commandName));

                keyDefinitions.Add(new KeyDefinition(new[] { commandName }, combination));
            }
        }
    }

    private string GetParamString(XmlNodeList paramNodes)
    {
        if (paramNodes == null || paramNodes.Count == 0) return string.Empty;

        StringBuilder builder = new StringBuilder();
        foreach (XmlNode paramNode in paramNodes)
        {
            if (paramNode.Attributes == null) continue;

            XmlAttribute paramName = paramNode.Attributes["name"];
            XmlAttribute paramValue = paramNode.Attributes["value"];
            if (paramName == null || paramValue == null) continue;

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
    private void LoadTools(XmlDocument document, List<KeyDefinition> keyDefinitions, bool applyDefaults)
    {
        XmlNodeList tools = document.SelectNodes("keyboard/tools/key");
        if (applyDefaults)
        {
            ApplyDefaults(keyDefinitions, "PixiEditor.Tools");
        }

        foreach (XmlNode tool in tools)
        {
            if (tool.Attributes == null) continue;

            XmlAttribute command = tool.Attributes["tool"];
            XmlAttribute shortcut = tool.Attributes["shortcut"];

            if (command == null || shortcut == null) continue;

            string commandName = command.Value;
            string shortcutValue = shortcut.Value;

            if (!Map.ContainsKey(commandName))
            {
                continue;
            }

            var mappedEntry = Map[commandName];
            foreach (var mappedCommand in mappedEntry.Commands)
            {
                commandName = mappedCommand;

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
                keyDefinitions.RemoveAll(x => x.Commands.Contains(commandName));

                keyDefinitions.Add(new KeyDefinition(new[] { commandName }, combination));
            }
        }
    }

    private void ApplyDefaults(List<KeyDefinition> keyDefinitions, string commandGroup)
    {
        foreach (var mapEntry in Map)
        {
            foreach (var command in mapEntry.Value.Commands)
            {
                if (command.StartsWith(commandGroup))
                {
                    keyDefinitions.Add(mapEntry.Value);
                }
            }
        }
    }
}
