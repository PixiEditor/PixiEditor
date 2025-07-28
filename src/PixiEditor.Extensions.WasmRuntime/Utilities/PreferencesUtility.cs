using PixiEditor.Extensions.CommonApi.Utilities;
using PixiEditor.Extensions.Metadata;

namespace PixiEditor.Extensions.WasmRuntime.Utilities;

internal static class PreferencesUtility
{
    public static void UpdateExtensionPreference<T>(Extension extension, string name, T value, bool updateLocalPreference)
    {
        if(name == null)
        {
            throw new ArgumentNullException(nameof(name));
        }
        
        string[] splitted = name.Split(":");

        if (splitted.Length > 2)
        {
            throw new ArgumentException("Name can't contain more than one ':' character. Valid schema is 'ExtensionUniqueName:PreferenceName' or 'PreferenceName'.");
        }
        
        string finalName = $"{extension.Metadata.UniqueName}:{name}";
        
        if (splitted.Length == 2)
        {
            string caller = splitted[0];
            
            bool triesToWriteExternal = caller != extension.Metadata.UniqueName;
            if (triesToWriteExternal)
            {
                PermissionUtility.ThrowIfLacksPermissions(extension.Metadata, ExtensionPermissions.WriteNonOwnedPreferences);
            }
            
            finalName = name;

            if(caller.Equals("pixieditor", StringComparison.CurrentCultureIgnoreCase)) 
            {
                finalName = splitted[1];
            }
        }

        if (updateLocalPreference)
        {
            extension.Api.Preferences.UpdateLocalPreference(finalName, value);
        }
        else
        {
            extension.Api.Preferences.UpdatePreference(finalName, value);
        }
    }
    
    public static T GetPreference<T>(Extension extension, string name, T fallbackValue, bool getLocalPreference)
    {
        if(name == null)
        {
            throw new ArgumentNullException(nameof(name));
        }
        
        string finalName = PrefixedNameUtility.ToPixiEditorRelativePreferenceName(extension.Metadata.UniqueName, name);

        if (getLocalPreference)
        {
            return extension.Api.Preferences.GetLocalPreference(finalName, fallbackValue);
        }

        return extension.Api.Preferences.GetPreference(finalName, fallbackValue);
    }
}
