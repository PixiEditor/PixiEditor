using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using PixiEditor.Extensions.Metadata;

namespace PixiEditor.Extensions;

/// <summary>
///     This class is used to extend the functionality of the PixiEditor.
/// </summary>
public abstract class Extension
{
    public ExtensionServices Api { get; private set; }
    public ExtensionMetadata Metadata { get; private set; }

    public void ProvideMetadata(ExtensionMetadata metadata)
    {
        if (Metadata != null)
        {
            return;
        }

        Metadata = metadata;
    }

    public void Load(ExtensionServices api)
    {
        Api = api;
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
