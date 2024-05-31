using Microsoft.CodeAnalysis;

namespace PixiEditor.Extensions.CommonApi.Diagnostics;

internal static class DiagnosticHelpers
{
    public static bool IsSettingType(TypeInfo info) => info.Type?.ContainingNamespace.ToString() == DiagnosticConstants.SettingNamespace && DiagnosticConstants.settingNames.Contains(info.Type.Name);
}
