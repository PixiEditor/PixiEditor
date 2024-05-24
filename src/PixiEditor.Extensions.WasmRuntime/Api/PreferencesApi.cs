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
    
    [ApiFunction("update_local_preference_int")]
    public void UpdateLocalPreference(string name, int value)
    {
        Api.Preferences.UpdateLocalPreference(name, value);
    }
    
    [ApiFunction("update_local_preference_bool")]
    public void UpdateLocalPreference(string name, bool value)
    {
        Api.Preferences.UpdateLocalPreference(name, value);
    }
    
    [ApiFunction("update_local_preference_string")]
    public void UpdateLocalPreference(string name, string value)
    {
        Api.Preferences.UpdateLocalPreference(name, value);
    }
    
    [ApiFunction("update_local_preference_float")]
    public void UpdateLocalPreference(string name, float value)
    {
        Api.Preferences.UpdateLocalPreference(name, value);
    }
    
    [ApiFunction("update_local_preference_double")]
    public void UpdateLocalPreference(string name, double value)
    {
        Api.Preferences.UpdateLocalPreference(name, value);
    }
    
    [ApiFunction("get_preference_int")]
    public int GetPreference(string name, int fallbackValue)
    {
        return Api.Preferences.GetPreference(name, fallbackValue);
    }
    
    [ApiFunction("get_preference_bool")]
    public bool GetPreference(string name, bool fallbackValue)
    {
        return Api.Preferences.GetPreference(name, fallbackValue);
    }
    
    [ApiFunction("get_preference_string")]
    public string GetPreference(string name, string fallbackValue)
    {
        return Api.Preferences.GetPreference(name, fallbackValue);
    }
    
    [ApiFunction("get_preference_float")]
    public float GetPreference(string name, float fallbackValue)
    {
        return Api.Preferences.GetPreference(name, fallbackValue);
    }
    
    [ApiFunction("get_preference_double")]
    public double GetPreference(string name, double fallbackValue)
    {
        return Api.Preferences.GetPreference(name, fallbackValue);
    }
    
    [ApiFunction("get_local_preference_int")]
    public int GetLocalPreference(string name, int fallbackValue)
    {
        return Api.Preferences.GetLocalPreference(name, fallbackValue);
    }
    
    [ApiFunction("get_local_preference_bool")]
    public bool GetLocalPreference(string name, bool fallbackValue)
    {
        return Api.Preferences.GetLocalPreference(name, fallbackValue);
    }
    
    [ApiFunction("get_local_preference_string")]
    public string GetLocalPreference(string name, string fallbackValue)
    {
        return Api.Preferences.GetLocalPreference(name, fallbackValue);
    }
    
    [ApiFunction("get_local_preference_float")]
    public float GetLocalPreference(string name, float fallbackValue)
    {
        return Api.Preferences.GetLocalPreference(name, fallbackValue);
    }
    
    [ApiFunction("get_local_preference_double")]
    public double GetLocalPreference(string name, double fallbackValue)
    {
        return Api.Preferences.GetLocalPreference(name, fallbackValue);
    }
}
