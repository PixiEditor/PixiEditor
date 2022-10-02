using System;
using System.Runtime.CompilerServices;

namespace PixiEditor.DrawingApi.Skia
{
    public static class CastUtility
    {
        public static unsafe T2[] UnsafeArrayCast<T1, T2>(T1[] source) where T2 : unmanaged
        {
            unsafe
            {
                T2[] target = new T2[source.Length];
                fixed (void* p = target)
                {
                    Unsafe.Copy(p, ref source);
                }
                
                return target;
            }
        }
    }
}
