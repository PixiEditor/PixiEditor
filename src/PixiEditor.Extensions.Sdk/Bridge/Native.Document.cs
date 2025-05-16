using System.Runtime.CompilerServices;

namespace PixiEditor.Extensions.Sdk.Bridge;

internal partial class Native
{
    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern string import_file(string path, bool associatePath);

    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern string get_active_document();

    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern void resize_document(string documentId, int width, int height);
}
