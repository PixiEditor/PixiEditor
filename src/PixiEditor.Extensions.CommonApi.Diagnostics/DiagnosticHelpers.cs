using Microsoft.CodeAnalysis;

namespace PixiEditor.Extensions.CommonApi.Diagnostics;

internal static class DiagnosticHelpers
{
    public static bool IsSettingType(TypeInfo info) => info.Type?.ContainingNamespace.ToString() == DiagnosticConstants.SettingNamespace && DiagnosticConstants.settingNames.Contains(info.Type.Name);

    public static string? GetPrefix(string name)
    {
        int colonPosition = name.IndexOf(':');

        return colonPosition == -1 ? null : name.Substring(0, colonPosition);
    }
    
    public static string? GetKey(string name)
    {
        int colonPosition = name.IndexOf(':');

        return colonPosition == -1 ? null : name.Substring(colonPosition + 1);
    }
}
