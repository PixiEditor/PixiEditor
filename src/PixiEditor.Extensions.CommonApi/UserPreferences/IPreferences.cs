namespace PixiEditor.Extensions.CommonApi.UserPreferences;

public interface IPreferences
{
    public static IPreferences Current { get; private set; }

    /// <summary>
    /// Saves the preferences to be stored permanently.
    /// </summary>
    public void Save();

    /// <summary>
    /// Adds a callback that will be executed when the setting called <paramref name="name"/> changes.
    /// </summary>
    /// <param name="name">The name of the setting</param>
    /// <param name="action">The action that will be executed when the setting changes</param>
    public void AddCallback(string name, Action<string, object> action);

    /// <summary>
    /// Adds a callback that will be executed when the setting called <paramref name="name"/> changes.
    /// </summary>
    /// <typeparam name="T">The <see cref="Type"/> of the setting</typeparam>
    /// <param name="name">The name of the setting</param>
    /// <param name="action">The action that will be executed when the setting changes</param>
    public void AddCallback<T>(string name, Action<string, T> action);

    public void RemoveCallback(string name, Action<string, object> action);
    public void RemoveCallback<T>(string name, Action<string, T> action);

    /// <summary>
    /// Initializes the preferences.
    /// </summary>
    public void Init();

    /// <summary>
    /// Initializes the preferences using the <paramref name="path"/> and <paramref name="localPath"/>
    /// </summary>
    public void Init(string path, string localPath);

    /// <summary>
    /// Updates a user preference and calls all added callbacks.
    /// </summary>
    /// <typeparam name="T">The <see cref="Type"/> of the setting</typeparam>
    /// <param name="name">The name of the setting.</param>
    /// <param name="value">The new value.</param>
    public void UpdatePreference<T>(string name, T value);

    /// <summary>
    /// Updates a editor setting and calls all added callbacks.
    /// </summary>
    /// <typeparam name="T">The <see cref="Type"/> of the setting</typeparam>
    /// <param name="name">The name of the setting</param>
    /// <param name="value">The new value</param>
    public void UpdateLocalPreference<T>(string name, T value);

#nullable enable

    /// <summary>
    /// Reads the user preference that is called <paramref name="name"/>, if the setting does not exist the default of <typeparamref name="T"/> will be used
    /// </summary>
    /// <typeparam name="T">The <see cref="Type"/> of the setting</typeparam>
    /// <param name="name">The name of the setting</param>
    /// <returns>The setting or the default of <typeparamref name="T"/> if it has not been set yet</returns>
    public T? GetPreference<T>(string name);

    /// <summary>
    /// Reads the user preference that is called <paramref name="name"/>, if the setting does not exist the default of <paramref name="fallbackValue"/> will be used
    /// </summary>
    /// <typeparam name="T">The <see cref="Type"/> of the setting</typeparam>
    /// <param name="name">The name of the setting</param>
    /// <returns>The setting or the <paramref name="fallbackValue"/> if it has not been set yet</returns>
    public T? GetPreference<T>(string name, T? fallbackValue);

    /// <summary>
    /// Reads the editor setting that is called <paramref name="name"/>, if the setting does not exist the deafult of <typeparamref name="T"/> will be used
    /// </summary>
    /// <typeparam name="T">The <see cref="Type"/> of the setting</typeparam>
    /// <param name="name">The name of the setting</param>
    /// <returns>The editor setting or the default of <typeparamref name="T"/> if it has not been set yet</returns>
    public T? GetLocalPreference<T>(string name);

    /// <summary>
    /// Reads the editor setting that is called <paramref name="name"/>, if the setting does not exist the <paramref name="fallbackValue"/> will be used
    /// </summary>
    /// <typeparam name="T">The <see cref="Type"/> of the setting</typeparam>
    /// <param name="name">The name of the setting</param>
    /// <returns>The editor setting or the <paramref name="fallbackValue"/> if it has not been set yet</returns>
    public T? GetLocalPreference<T>(string name, T? fallbackValue);

    protected static void SetAsCurrent(IPreferences provider)
    {
        Current = provider;
    }
}
