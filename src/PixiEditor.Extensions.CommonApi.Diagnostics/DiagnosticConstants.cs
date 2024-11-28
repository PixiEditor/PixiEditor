using System.Collections.Generic;

namespace PixiEditor.Extensions.CommonApi.Diagnostics;

internal static class DiagnosticConstants
{
    public const string Category = "PixiEditor.CommonAPI";
    
    public const string SettingNamespace = "PixiEditor.Extensions.CommonApi.UserPreferences.Settings";
    public static List<string> settingNames = ["SyncedSetting", "LocalSetting"];
}
