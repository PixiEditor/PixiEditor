using PixiEditor.Extensions.CommonApi.UserPreferences;
using PixiEditor.Extensions.CommonApi.Utilities;
using PixiEditor.Extensions.WasmRuntime.Utilities;

namespace PixiEditor.Extensions.WasmRuntime.Api.Modules;

internal class PreferencesModule : ApiModule
{
    private IPreferences Preferences { get; }
    public PreferencesModule(WasmExtensionInstance extension, IPreferences preferences) : base(extension)
    {
        Preferences = preferences;
    }

    public void AddPreferenceCallback(string name)
    {
        string prefixedName = PrefixedNameUtility.ToPixiEditorRelativePreferenceName(Extension.Metadata.UniqueName, name);
        Preferences.AddCallback(prefixedName, InvokeExtensionCallback);
    }
    
    public void RemovePreferenceCallback(string name)
    {
        string prefixedName = PrefixedNameUtility.ToPixiEditorRelativePreferenceName(Extension.Metadata.UniqueName, name);
        Preferences.RemoveCallback(prefixedName, InvokeExtensionCallback);
    }

    private void InvokeExtensionCallback(string preferenceName, object value)
    {
        string returnName = PrefixedNameUtility.ToExtensionRelativeName(Extension.Metadata.UniqueName, preferenceName);
        if (value.GetType().Name.Equals("string", StringComparison.InvariantCultureIgnoreCase))
        {
            InvokeStringCallback(preferenceName, value);
        }
        else if (value.GetType().Name.Equals("byte[]", StringComparison.InvariantCultureIgnoreCase))
        {
            InvokeByteArrayCallback(preferenceName, value);
        }
        else
        {
            InvokeNonLengthCallback(preferenceName, value);
        }
    }

    private void InvokeStringCallback(string preferenceName, object value)
    {
        string stringValue = (string)value;
        var callbackAction = Extension.Instance.GetAction<int, int>("string_preference_updated");
        int valuePtr = Extension.WasmMemoryUtility.WriteString(stringValue);
        int namePtr = Extension.WasmMemoryUtility.WriteString(preferenceName);
            
        callbackAction.Invoke(namePtr, valuePtr);
    }
    
    private void InvokeByteArrayCallback(string preferenceName, object value)
    {
        byte[] byteArrayValue = (byte[])value;
        var callbackAction = Extension.Instance.GetAction<int, int, int>("byte_array_preference_updated");
        int valuePtr = Extension.WasmMemoryUtility.WriteSpan(byteArrayValue);
        int namePtr = Extension.WasmMemoryUtility.WriteString(preferenceName);
            
        callbackAction.Invoke(namePtr, valuePtr, byteArrayValue.Length);
    }
    
    private void InvokeNonLengthCallback(string preferenceName, object value)
    {
        bool isValid = value is int or bool or float or double;
        if (!isValid)
        {
            throw new ArgumentException("Unsupported preference value type.");
        }
        
        string typeName = value.GetType().Name.ToLower();
        int namePtr = Extension.WasmMemoryUtility.WriteString(preferenceName);
        InvokePrimitiveCallbackAction(typeName, value, namePtr);
    }

    private void InvokePrimitiveCallbackAction(string typeName, object value, int namePtr)
    {
        switch (value)
        {
            case int intValue:
                Extension.Instance.GetAction<int, int>("int32_preference_updated").Invoke(namePtr, intValue);
                break;
            case bool boolValue:
                Extension.Instance.GetAction<int, int>("bool_preference_updated").Invoke(namePtr, Convert.ToInt32(boolValue));
                break;
            case float floatValue:
                Extension.Instance.GetAction<int, float>("float_preference_updated").Invoke(namePtr, floatValue);
                break;
            case double doubleValue:
                Extension.Instance.GetAction<int, double>("double_preference_updated").Invoke(namePtr, doubleValue);
                break;
        }
    }
}
