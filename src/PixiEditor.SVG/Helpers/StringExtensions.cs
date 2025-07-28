namespace PixiEditor.SVG.Helpers;

public static class StringExtensions
{
    public static string ToKebabCase(this string str)
    {
        return string.Concat(str.Select((x, i) => i > 0 && char.IsUpper(x) ? "-" + x.ToString() : x.ToString())).ToLower();
    }
    
    public static string FromKebabToTitleCase(this string str)
    {
        return string.Concat(str.Split('-').Select(x => char.ToUpper(x[0]) + x.Substring(1)));
    }
}
