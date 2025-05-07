using System.Runtime.CompilerServices;

namespace PixiEditor.Extensions.Metadata;

[Flags]
[Newtonsoft.Json.JsonConverter(typeof(JsonEnumFlagConverter))]
public enum ExtensionPermissions
{
    None = 0,

    /// <summary>
    ///     Allows extension to write to preferences that are not owned by the extension. Owned preferences are those that are
    ///    created by the extension itself (they are prefixed with the extension unique name, ex. PixiEditor.SomeExt:PopupShown).
    /// </summary>
    WriteNonOwnedPreferences = 1,

    /// <summary>
    ///     Allows extension to open documents. This permission is required for extensions that need to import files into
    ///     the editor.
    /// </summary>
    OpenDocuments = 2,
    FullAccess = ~0,
}
