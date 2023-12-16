using System.IO;
using System.Text;
using System.Xml;
using PixiEditor.Extensions.Palettes;
using PixiEditor.Extensions.Palettes.Parsers;

namespace PixiEditor.Models.IO.PaletteParsers.CorelXmlFile;

internal class CorelXmlParser : PaletteFileParser
{
    public override string FileName { get; } = "Corel XML Palette";
    public override string[] SupportedFileExtensions { get; } = { ".xml" };

    private enum State
    {
        None,
        Palette,
        Colors,
        Page,
        Color,
    }

    private const string _tintFormat = "0.###############";
    private readonly char[] _tintSeparators = new char[1] { ',' };

    public override async Task<PaletteFileData> Parse(string path)
    {
        try
        {
            return await ParseFile(path);
        }
        catch
        {
            return PaletteFileData.Corrupted;
        }
    }

    private async Task<PaletteFileData> ParseFile(string path)
    {
        var lines = await ReadTextLines(path);
        string name = Path.GetFileNameWithoutExtension(path);

        List<List<PaletteColor>> colorGroups = new();

        await using (Stream stream = File.OpenRead(path))
        {
            using (TextReader input = new StreamReader(stream, Encoding.UTF8))
            {
                using (XmlReader reader = XmlReader.Create(input))
                {
                    State state = State.None;
                    List<PaletteColor> colors = new();

                    do
                    {
                        if (reader.Read())
                        {
                            switch (reader.NodeType)
                            {
                                case XmlNodeType.Element:
                                    if (state == State.None && IsPaletteNode(reader))
                                    {
                                        state = State.Palette;
                                        break;
                                    }
                                    if (state == State.Palette && IsColorsNode(reader))
                                    {
                                        state = State.Colors;
                                        break;
                                    }
                                    if (state == State.Colors && IsPageNode(reader))
                                    {
                                        colorGroups.Add(colors);
                                        state = State.Page;
                                        break;
                                    }
                                    if (state == State.Page && IsColorNode(reader))
                                    {
                                        this.ReadColor(reader.ReadSubtree(), colors);
                                        break;
                                    }
                                    break;
                                case XmlNodeType.EndElement:
                                    switch (state)
                                    {
                                        case State.Palette:
                                            state = State.None;
                                            break;
                                        case State.Colors:
                                            if (IsColorsNode(reader))
                                            {
                                                state = State.Palette;
                                                break;
                                            }
                                            break;
                                        case State.Page:
                                            if (IsPageNode(reader))
                                            {
                                                state = State.Colors;
                                                colors = null;
                                                break;
                                            }
                                            break;
                                    }
                                    break;
                            }
                        }
                    }
                    while (!reader.EOF);
                }
            }
        }

        // flatten color groups into single list
        var colorList = colorGroups.SelectMany(i => i);

        System.Windows.MessageBox.Show(colorList.Count().ToString());

        return new PaletteFileData(name, colorList.ToArray());
    }

    public override async Task<bool> Save(string path, PaletteFileData data)
    {
        StringBuilder sb = new();
        string name = string.IsNullOrEmpty(data.Title) ? Path.GetFileNameWithoutExtension(path) : data.Title;
        sb.AppendLine("GIMP Palette");
        sb.AppendLine($"#Name: {name}");
        sb.AppendLine($"#Colors {data.Colors.Length}");
        sb.AppendLine("#Made with PixiEditor");
        sb.AppendLine("#");
        foreach (var color in data.Colors)
        {
            string hex = $"{color.R:X}{color.G:X}{color.B:X}";
            sb.AppendLine($"{color.R}\t{color.G}\t{color.B}\t{hex}");
        }

        try
        {
            await File.WriteAllTextAsync(path, sb.ToString());
            return true;
        }
        catch
        {
            return false;
        }
    }

    private void BuildColor(List<PaletteColor> colors, string name, string colorspace, string tints)
    {
        string[] strArray = tints.Split(_tintSeparators);

        if (string.Equals(colorspace, "rgb", StringComparison.OrdinalIgnoreCase))
        {
            colors.Add(new PaletteColor(Convert.ToByte(strArray[0]), Convert.ToByte(strArray[1]), Convert.ToByte(strArray[2])));
        }
        else if (string.Equals(colorspace, "cmyk", StringComparison.OrdinalIgnoreCase))
        {
            colors.Add(new PaletteColor(Convert.ToByte(strArray[0]), Convert.ToByte(strArray[1]), Convert.ToByte(strArray[2])));
        }
        else if (string.Equals(colorspace, "cmy", StringComparison.OrdinalIgnoreCase))
        {
            colors.Add(new PaletteColor(Convert.ToByte(strArray[0]), Convert.ToByte(strArray[1]), Convert.ToByte(strArray[2])));
        }
        else
        {
            if (!string.Equals(colorspace, "hls", StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException(string.Format("Color space '{0}' not supported.", colorspace));

            colors.Add(new PaletteColor(Convert.ToByte(strArray[0]), Convert.ToByte(strArray[2]), Convert.ToByte(strArray[1])));
        }
    }

    private bool IsColorNode(XmlReader reader) => string.Equals(reader.Name, "color", StringComparison.OrdinalIgnoreCase);

    private bool IsColorsNode(XmlReader reader) => string.Equals(reader.Name, "colors", StringComparison.OrdinalIgnoreCase);

    private bool IsColorSpaceAttribute(XmlReader reader) => string.Equals(reader.Name, "cs", StringComparison.OrdinalIgnoreCase);

    private bool IsNameAttribute(XmlReader reader) => string.Equals(reader.Name, "name", StringComparison.OrdinalIgnoreCase);

    private bool IsPageNode(XmlReader reader) => string.Equals(reader.Name, "page", StringComparison.OrdinalIgnoreCase);

    private bool IsPaletteNode(XmlReader reader) => string.Equals(reader.Name, "palette", StringComparison.OrdinalIgnoreCase);

    private bool IsTintsAttribute(XmlReader reader) => string.Equals(reader.Name, "tints", StringComparison.OrdinalIgnoreCase);

    private void ReadColor(XmlReader reader, List<PaletteColor> colors)
    {
        if (!reader.Read()) return;

        string colorspace = string.Empty;
        string tints = string.Empty;
        string name = string.Empty;
        while (reader.MoveToNextAttribute())
        {
            if (IsNameAttribute(reader))
            {
                name = reader.Value;
            }
            else if (IsColorSpaceAttribute(reader))
            {
                colorspace = reader.Value;
            }
            else if (IsTintsAttribute(reader))
            {
                tints = reader.Value;
            }
        }
        BuildColor(colors, name, colorspace, tints);
    }

    // TODO
    private void WritePage(XmlWriter writer, List<PaletteColor> colors)
    {

    }

    // TODO
    private void WriteColor(XmlWriter writer, PaletteColor color)
    {
        StringBuilder sb = new StringBuilder(32);

        sb.Append(color.R);
        sb.Append(',');
        sb.Append(color.G);
        sb.Append(',');
        sb.Append(color.B);
    }
}
