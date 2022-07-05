namespace PixiEditor.Helpers;

internal static class StringExtensions
{
    public static string Reverse(this string s)
    {
        return new string(s.Reverse<char>().ToArray());
    }
}
