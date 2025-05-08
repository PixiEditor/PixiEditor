using System.Text;

namespace PixiEditor.Extensions.CommonApi.FlyUI.Properties;

public struct TextStyle : IStructProperty
{
    // IMPORTANT: If you change this struct, you must also change the version below and handle
    // deserialization in the FlyUI deserializer.
    public const int Version = 1;
    public string FontFamily { get; set; } = "";
    public double FontSize { get; set; } = 12;
    public FontStyle FontStyle { get; set; } = FontStyle.Normal;
    public FontWeight FontWeight { get; set; } = FontWeight.Normal;
    public Color Color { get; set; } = Colors.White;

    public static TextStyle Default => new TextStyle("", 12, FontStyle.Normal, FontWeight.Normal, Colors.White);

    public TextStyle(string fontFamily = "", double fontSize = 12, FontStyle fontStyle = FontStyle.Normal,
        FontWeight fontWeight = FontWeight.Normal, Color? color = null)
    {
        FontFamily = fontFamily;
        FontSize = fontSize;
        FontStyle = fontStyle;
        FontWeight = fontWeight;
        Color = color ?? Colors.White;
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

        data.AddRange(BitConverter.GetBytes(FontSize));
        data.Add((byte)FontStyle);
        data.AddRange(BitConverter.GetBytes((int)FontWeight));
        data.AddRange(((IStructProperty)Color).Serialize());
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
        }
        else
        {
            FontFamily = "$Default";
        }

        index += fontFamilyLength;
        FontSize = BitConverter.ToDouble(data, index);
        index += 8;
        FontStyle = (FontStyle)data[index++];
        FontWeight = (FontWeight)BitConverter.ToInt32(data, index);
        index += 4;
        Color = Color.FromBytes(data[index..]);
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
