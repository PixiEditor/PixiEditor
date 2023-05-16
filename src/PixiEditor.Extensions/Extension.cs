using System.Reflection;
using PixiEditor.Extensions.Metadata;

namespace PixiEditor.Extensions;

/// <summary>
///     This class is used to extend the functionality of the PixiEditor.
/// </summary>
public abstract class Extension
{
    public ExtensionMetadata Metadata { get; private set; }

    public Action<string, string> NoticeDialogImpl { get; set; }
    public void NoticeDialog(string message, string title)
    {
        NoticeDialogImpl?.Invoke(message, title);
    }

    public void ProvideMetadata(ExtensionMetadata metadata)
    {
        if (Metadata != null)
        {
            return;
        }

        Metadata = metadata;
    }

    public void Load()
    {
        OnLoaded();
    }

    public void Initialize()
    {
        OnInitialized();
    }

    /// <summary>
    ///     Called right after the extension is loaded. Not all extensions are initialized at this point.
    /// </summary>
    protected virtual void OnLoaded()
    {
    }

    /// <summary>
    ///     Called after all extensions and PixiEditor is loaded. All extensions are initialized at this point.
    /// </summary>
    protected virtual void OnInitialized()
    {
    }
}
