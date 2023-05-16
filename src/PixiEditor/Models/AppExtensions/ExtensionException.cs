using PixiEditor.Exceptions;
using PixiEditor.Models.Localization;

namespace PixiEditor.Models.AppExtensions;

public class ExtensionException : RecoverableException
{
    public ExtensionException(LocalizedString messageKey) : base(messageKey)
    {
    }
}

public class NoEntryAssemblyException : ExtensionException
{
    public NoEntryAssemblyException(string containingFolder) : base(new LocalizedString("ERROR_NO_ENTRY_ASSEMBLY", containingFolder))
    {
    }
}

public class NoClassEntryException : ExtensionException
{
    public NoClassEntryException(string assemblyPath) : base(new LocalizedString("ERROR_NO_CLASS_ENTRY", assemblyPath))
    {
    }
}

public class MissingMetadataException : ExtensionException
{
    public MissingMetadataException(string missingMetadataKey) : base(new LocalizedString("ERROR_MISSING_METADATA", missingMetadataKey))
    {
    }
}

