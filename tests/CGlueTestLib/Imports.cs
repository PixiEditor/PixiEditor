using System.Runtime.CompilerServices;

namespace CGlueTestLib;

internal static class Imports
{
    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern string string_return_method();
}
