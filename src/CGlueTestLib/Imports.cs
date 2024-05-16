using System.Runtime.InteropServices;

namespace CGlueTestLib;

internal static class Imports
{
    [DllImport("pixieditor")]
    internal static extern void test();
}
