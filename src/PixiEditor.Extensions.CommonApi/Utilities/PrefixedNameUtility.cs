namespace PixiEditor.Extensions.CommonApi.Utilities;

public static class PrefixedNameUtility
{
    /// <summary>
    ///     Converts a preference name to a full name with the extension unique name. It is relative to PixiEditor, so
    /// any preference without a prefix is a PixiEditor preference.
    /// </summary>
    /// <param name="uniqueName">Unique name of the extension.</param>
    /// <param name="name">Name of the preference.</param>
    /// <returns>Full name of the preference.</returns>
    public static string ToPixiEditorRelativePreferenceName(string uniqueName, string name)
    {
        string[] splitted = name.Split(":");
        
        string finalName = $"{uniqueName}:{name}";
        
        if (splitted.Length == 2)
        {
            finalName = name;
            
            if(splitted[0].Equals("pixieditor", StringComparison.CurrentCultureIgnoreCase))
            {
                finalName = splitted[1];
            }
        }

        return finalName;
    }

    /// <summary>
    ///    It is a reverse of <see cref="ToPixiEditorRelativePreferenceName"/>. It converts a full preference name to a relative name.
    /// Preferences owned by the extension will not have any prefix, while PixiEditor preferences will have "pixieditor:" prefix.
    /// </summary>
    /// <param name="extensionUniqueName">Unique name of the extension.</param>
    /// <param name="preferenceName">Full name of the preference.</param>
    /// <returns>Relative name of the preference.</returns>
    public static string ToExtensionRelativeName(string extensionUniqueName, string preferenceName)
    {
        if (preferenceName.StartsWith(extensionUniqueName))
        {
            return preferenceName[(extensionUniqueName.Length + 1)..];
        }

        if(preferenceName.Split(":").Length == 1)
        {
            return $"pixieditor:{preferenceName}";
        }

        return preferenceName;
    }

    public static string ToCommandUniqueName(string extensionUniqueName, string metadataUniqueName)
    {
        if (metadataUniqueName.StartsWith(extensionUniqueName))
        {
            return metadataUniqueName;
        }

        return $"{extensionUniqueName}:{metadataUniqueName}";
    }
}
