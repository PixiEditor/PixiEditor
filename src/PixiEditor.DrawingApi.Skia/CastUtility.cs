using System;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace PixiEditor.DrawingApi.Skia
{
    public static class CastUtility
    {
        public static T2[] UnsafeArrayCast<T1, T2>(T1[] source) where T1 : struct where T2 : struct
        {
            return MemoryMarshal.Cast<T1, T2>(source).ToArray();
        }
    }
}
