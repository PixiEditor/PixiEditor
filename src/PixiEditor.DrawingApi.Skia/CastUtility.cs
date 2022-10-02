using System.Runtime.CompilerServices;

namespace PixiEditor.DrawingApi.Skia
{
    public static class CastUtility
    {
        public static T2[] UnsafeArrayCast<T1, T2>(T1[] source)
        {
            unsafe
            {
                T2[] target = new T2[source.Length];
                fixed (void* p = &target)
                {
                    Unsafe.Copy(p, ref source);
                }
                
                return target;
            }
        }
    }
}
