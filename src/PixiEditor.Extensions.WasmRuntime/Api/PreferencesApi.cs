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
        Api.Preferences.UpdatePreference(name, value);
    }
    
    [ApiFunction("update_preference_bool")]
    public void UpdatePreference(string name, bool value)
    {
        Api.Preferences.UpdatePreference(name, value);
    }
    
    [ApiFunction("update_preference_string")]
    public void UpdatePreference(string name, string value)
    {
        Api.Preferences.UpdatePreference(name, value);
    }
    
    [ApiFunction("update_preference_float")]
    public void UpdatePreference(string name, float value)
    {
        Api.Preferences.UpdatePreference(name, value);
    }
    
    [ApiFunction("update_preference_double")]
    public void UpdatePreference(string name, double value)
    {
        Api.Preferences.UpdatePreference(name, value);
    }
}
