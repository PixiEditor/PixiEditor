using System.Text;

namespace PixiEditor.Extensions.CommonApi.FlyUI.Properties;

public struct TextStyle : IStructProperty
{
    // IMPORTANT: If you change this struct, you must also change the version below and handle
    // deserialization in the FlyUI deserializer.
    public const int Version = 1;
    public string? FontFamily { get; set; }
    public double? FontSize { get; set; }
    public FontStyle? FontStyle { get; set; }
    public FontWeight? FontWeight { get; set; }
    public Color? Color { get; set; }

    public static TextStyle Default => new TextStyle(null, null, null, null, null);

    public TextStyle(string? fontFamily = null, double? fontSize = null, FontStyle? fontStyle = null, FontWeight? fontWeight = null, Color? color = null)
    {
        FontFamily = fontFamily;
        FontSize = fontSize;
        FontStyle = fontStyle;
        FontWeight = fontWeight;
        Color = color;
    }

    public byte[] Serialize()
    {
        var data = new List<byte>();
        data.AddRange(BitConverter.GetBytes(Version));
        data.AddRange(BitConverter.GetBytes(FontFamily?.Length ?? 0));
        if (FontFamily != null)
        {
            data.AddRange(Encoding.UTF8.GetBytes(FontFamily));
        }

        data.Add(FontSize != null ? (byte)1 : (byte)0);
        if (FontSize != null)
        {
            data.AddRange(BitConverter.GetBytes(FontSize.Value));
        }

        data.Add(FontStyle != null ? (byte)1 : (byte)0);
        if (FontStyle != null)
        {
            data.Add((byte)FontStyle.Value);
        }

        data.Add(FontWeight != null ? (byte)1 : (byte)0);
        if (FontWeight != null)
        {
            data.AddRange(BitConverter.GetBytes((int)FontWeight.Value));
        }

        data.Add(Color != null ? (byte)1 : (byte)0);
        if (Color != null)
        {
            data.AddRange(((IStructProperty)Color).Serialize());
        }

        return data.ToArray();
    }

    public void Deserialize(byte[] data)
    {
        int index = 0;
        int version = BitConverter.ToInt32(data, index);
        index += 4;
        int fontFamilyLength = BitConverter.ToInt32(data, index);
        index += 4;
        if (fontFamilyLength > 0)
        {
            FontFamily = Encoding.UTF8.GetString(data, index, fontFamilyLength);
            index += fontFamilyLength;
        }
        else
        {
            FontFamily = null;
        }

        bool hasFontSize = data[index] == 1;
        index++;
        if (hasFontSize)
        {
            FontSize = BitConverter.ToDouble(data, index);
            index += 8;
        }
        else
        {
            FontSize = null;
        }

        bool hasFontStyle = data[index] == 1;
        index++;
        if (hasFontStyle)
        {
            FontStyle = (FontStyle)data[index];
            index++;
        }
        else
        {
            FontStyle = null;
        }

        bool hasFontWeight = data[index] == 1;
        index++;
        if (hasFontWeight)
        {
            FontWeight = (FontWeight)BitConverter.ToInt32(data, index);
            index += 4;
        }
        else
        {
            FontWeight = null;
        }

        bool hasColor = data[index] == 1;
        index++;
        if (hasColor)
        {
            Color = Properties.Color.FromBytes(data[index..]);
        }
        else
        {
            Color = null;
        }
    }
}

public enum FontStyle
{
    Normal,
    Italic,
    Oblique
}

public enum FontWeight
{
    Thin = 100,
    ExtraLight = 200,
    Light = 300,
    Normal = 400,
    Medium = 500,
    SemiBold = 600,
    Bold = 700,
    ExtraBold = 800,
    Black = 900
}
