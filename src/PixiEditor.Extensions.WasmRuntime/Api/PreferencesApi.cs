using PixiEditor.Extensions.CommonApi.Utilities;
using PixiEditor.Extensions.Metadata;
using PixiEditor.Extensions.WasmRuntime.Api.Modules;
using PixiEditor.Extensions.WasmRuntime.Utilities;

namespace PixiEditor.Extensions.WasmRuntime.Api;

internal class PreferencesApi : ApiGroupHandler
{
    [ApiFunction("save_preferences")]
    public void SavePreferences()
    {
        Api.Preferences.Save();
    }
    
    [ApiFunction("update_preference_int")]
    public void UpdatePreference(string name, int value)
    {
        PreferencesUtility.UpdateExtensionPreference(Extension, name, value, false);
    }
    
    [ApiFunction("update_preference_bool")]
    public void UpdatePreference(string name, bool value)
    {
        PreferencesUtility.UpdateExtensionPreference(Extension, name, value, false);
    }
    
    [ApiFunction("update_preference_string")]
    public void UpdatePreference(string name, string value)
    {
        PreferencesUtility.UpdateExtensionPreference(Extension, name, value, false);
    }
    
    [ApiFunction("update_preference_float")]
    public void UpdatePreference(string name, float value)
    {
        PreferencesUtility.UpdateExtensionPreference(Extension, name, value, false);
    }
    
    [ApiFunction("update_preference_double")]
    public void UpdatePreference(string name, double value)
    {
        PreferencesUtility.UpdateExtensionPreference(Extension, name, value, false);
    }
    
    [ApiFunction("update_local_preference_int")]
    public void UpdateLocalPreference(string name, int value)
    {
        PreferencesUtility.UpdateExtensionPreference(Extension, name, value, true);
    }
    
    [ApiFunction("update_local_preference_bool")]
    public void UpdateLocalPreference(string name, bool value)
    {
        PreferencesUtility.UpdateExtensionPreference(Extension, name, value, true);
    }
    
    [ApiFunction("update_local_preference_string")]
    public void UpdateLocalPreference(string name, string value)
    {
        PreferencesUtility.UpdateExtensionPreference(Extension, name, value, true);
    }
    
    [ApiFunction("update_local_preference_float")]
    public void UpdateLocalPreference(string name, float value)
    {
        PreferencesUtility.UpdateExtensionPreference(Extension, name, value, true);
    }
    
    [ApiFunction("update_local_preference_double")]
    public void UpdateLocalPreference(string name, double value)
    {
        PreferencesUtility.UpdateExtensionPreference(Extension, name, value, true);
    }
    
    [ApiFunction("get_preference_int")]
    public int GetPreference(string name, int fallbackValue)
    {
        var result = PreferencesUtility.GetPreference(Extension, name, fallbackValue, false);
        return result;
    }
    
    [ApiFunction("get_preference_bool")]
    public bool GetPreference(string name, bool fallbackValue)
    {
        var result = PreferencesUtility.GetPreference(Extension, name, fallbackValue, false);
        return result;
    }
    
    [ApiFunction("get_preference_string")]
    public string GetPreference(string name, string fallbackValue)
    {
        var result = PreferencesUtility.GetPreference(Extension, name, fallbackValue, false);
        return result;
    }
    
    [ApiFunction("get_preference_float")]
    public float GetPreference(string name, float fallbackValue)
    {
        var result = PreferencesUtility.GetPreference(Extension, name, fallbackValue, false);
        return result;
    }
    
    [ApiFunction("get_preference_double")]
    public double GetPreference(string name, double fallbackValue)
    {
        var result = PreferencesUtility.GetPreference(Extension, name, fallbackValue, false);
        return result;
    }
    
    [ApiFunction("get_local_preference_int")]
    public int GetLocalPreference(string name, int fallbackValue)
    {
        var result = PreferencesUtility.GetPreference(Extension, name, fallbackValue, true);
        return result;
    }
    
    [ApiFunction("get_local_preference_bool")]
    public bool GetLocalPreference(string name, bool fallbackValue)
    {
        var result = PreferencesUtility.GetPreference(Extension, name, fallbackValue, true);
        return result;
    }
    
    [ApiFunction("get_local_preference_string")]
    public string GetLocalPreference(string name, string fallbackValue)
    {
        var result = PreferencesUtility.GetPreference(Extension, name, fallbackValue, true);
        return result;
    }
    
    [ApiFunction("get_local_preference_float")]
    public float GetLocalPreference(string name, float fallbackValue)
    {
        var result = PreferencesUtility.GetPreference(Extension, name, fallbackValue, true);
        return result;
    }
    
    [ApiFunction("get_local_preference_double")]
    public double GetLocalPreference(string name, double fallbackValue)
    {
        var result = PreferencesUtility.GetPreference(Extension, name, fallbackValue, true);
        return result;
    }
    
    [ApiFunction("add_preference_callback")]
    public void AddPreferenceCallback(string name)
    {
        Extension.GetModule<PreferencesModule>().AddPreferenceCallback(name);
    }
    
    [ApiFunction("remove_preference_callback")]
    public void RemovePreferenceCallback(string name)
    {
        Extension.GetModule<PreferencesModule>().RemovePreferenceCallback(name);
    }
}
