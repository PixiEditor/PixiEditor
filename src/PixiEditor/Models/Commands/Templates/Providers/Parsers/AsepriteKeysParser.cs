using System.IO;
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

    private static ShortcutsTemplate LoadAndParse(string path)
    {
        XmlDocument doc = new XmlDocument();
        doc.Load(path);
        
        ShortcutsTemplate template = new ShortcutsTemplate();
        return template;
    }
}
