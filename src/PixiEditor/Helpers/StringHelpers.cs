using System.Text;

namespace PixiEditor.Helpers;

internal static class StringHelpers
{
    public static string AddSpacesBeforeUppercaseLetters(this string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return "";

        StringBuilder newText = new StringBuilder(text.Length * 2);
        newText.Append(text[0]);
        for (int i = 1; i < text.Length; i++)
        {
            if (char.IsUpper(text[i]) && text[i - 1] != ' ')
                newText.Append(' ');
            newText.Append(text[i]);
        }
        return newText.ToString();
    }

    public static string Limit(this string value, int maxLenght)
    {
        return value.Length > maxLenght ? value.Substring(0, maxLenght) : value;
    }
}
