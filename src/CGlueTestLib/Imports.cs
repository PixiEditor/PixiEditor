using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CGlueTestLib;

internal static class Imports
{
    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern void test();
}
