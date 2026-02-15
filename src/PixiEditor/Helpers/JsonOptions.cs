using System.Text.Json;

namespace PixiEditor.Helpers;

public static class JsonOptions
{
    public static JsonSerializerOptions CasesInsensitive { get; } =
        new()
        {
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        };
    
    public static JsonSerializerOptions CasesInsensitiveIndented { get; } =
        new()
        {
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            WriteIndented = true
        };
}
