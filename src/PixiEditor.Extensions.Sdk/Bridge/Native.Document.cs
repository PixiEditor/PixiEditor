using System.Runtime.CompilerServices;

namespace PixiEditor.Extensions.Sdk.Bridge;

internal partial class Native
{
    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern void import_file(string path, bool associatePath);
}
