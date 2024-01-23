using System.Reflection;
using PixiEditor.Extensions;

namespace PixiEditor.AvaloniaUI.Models.AppExtensions;

public class DllExtensionEntry : ExtensionEntry
{
    public Assembly Assembly { get; }
    public Type ExtensionType { get; }
    public DllExtensionEntry(Assembly assembly, Type extensionType)
    {
        Assembly = assembly;
        ExtensionType = extensionType;
    }

    public override Extension CreateExtension()
    {
        var extension = (Extension)Activator.CreateInstance(ExtensionType);
        if (extension is null)
        {
            throw new NoClassEntryException(Assembly.Location);
        }

        return extension;
    }
}