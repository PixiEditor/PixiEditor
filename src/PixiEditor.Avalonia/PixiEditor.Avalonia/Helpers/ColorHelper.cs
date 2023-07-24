using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using System.Windows;
using Avalonia.Input;

namespace PixiEditor.Helpers;

public class ColorHelper
{
    public static bool ParseAnyFormat(IDataObject data, [NotNullWhen(true)] out DrawingApi.Core.ColorsImpl.Color? result) => 
        ParseAnyFormat(((DataObject)data).GetText().Trim(), out result);
    
    public static bool ParseAnyFormatList(IDataObject data, [NotNullWhen(true)] out List<DrawingApi.Core.ColorsImpl.Color> result) => 
        ParseAnyFormatList(((DataObject)data).GetText().Trim(), out result);

    public static bool ParseAnyFormat(string value, [NotNullWhen(true)] out DrawingApi.Core.ColorsImpl.Color? result)
    {
        bool hex = Regex.IsMatch(value, "^#?([a-fA-F0-9]{8}|[a-fA-F0-9]{6}|[a-fA-F0-9]{3})$");

        if (hex)
        {
            result = DrawingApi.Core.ColorsImpl.Color.Parse(value);
            return true;
        }

        var match = Regex.Match(value, @"(?:rgba?\(?)? *(?<r>\d{1,3})(?:, *| +)(?<g>\d{1,3})(?:, *| +)(?<b>\d{1,3})(?:(?:, *| +)(?<a>\d{0,3}))?\)?");

        if (!match.Success)
        {
            result = null;
            return false;
        }

        byte r = byte.Parse(match.Groups["r"].ValueSpan);
        byte g = byte.Parse(match.Groups["g"].ValueSpan);
        byte b = byte.Parse(match.Groups["b"].ValueSpan);
        byte a = match.Groups["a"].Success ? byte.Parse(match.Groups["a"].ValueSpan) : (byte)255;

        result = new DrawingApi.Core.ColorsImpl.Color(r, g, b, a);
        return true;
    }
    
    public static bool ParseAnyFormatList(string value, [NotNullWhen(true)] out List<DrawingApi.Core.ColorsImpl.Color> result)
    {
        result = new List<DrawingApi.Core.ColorsImpl.Color>();

        // Regex patterns for hex and RGB(A) formats
        const string hexPattern = @"#?([a-fA-F0-9]{8}|[a-fA-F0-9]{6}|[a-fA-F0-9]{3})";
        const string rgbaPattern = @"(?:rgba?\(?)? *(?<r>\d{1,3})(?:, *| +)(?<g>\d{1,3})(?:, *| +)(?<b>\d{1,3})(?:(?:, *| +)(?<a>\d{0,3}))?\)?";

        // Combined pattern for both hex and RGB(A) formats
        const string combinedPattern = $@"({hexPattern})|({rgbaPattern})";
        var matches = Regex.Matches(value, combinedPattern);

        if (matches.Count == 0)
        {
            return false;
        }

        foreach (Match match in matches)
        {
            if (Regex.IsMatch(match.Value, $"^{hexPattern}$"))
            {
                result.Add(DrawingApi.Core.ColorsImpl.Color.Parse(match.Value));
            }
            else if (match.Groups["r"].Success && match.Groups["g"].Success && match.Groups["b"].Success)
            {
                byte r = byte.Parse(match.Groups["r"].ValueSpan);
                byte g = byte.Parse(match.Groups["g"].ValueSpan);
                byte b = byte.Parse(match.Groups["b"].ValueSpan);
                byte a = match.Groups["a"].Success ? byte.Parse(match.Groups["a"].ValueSpan) : (byte)255;

                result.Add(new DrawingApi.Core.ColorsImpl.Color(r, g, b, a));
            }
        }

        return result.Count > 0;
    }
}
