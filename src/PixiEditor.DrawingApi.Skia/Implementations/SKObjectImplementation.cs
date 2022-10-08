using System;
using System.Collections.Generic;
using SkiaSharp;

namespace PixiEditor.DrawingApi.Skia.Implementations
{
    public abstract class SkObjectImplementation<T> where T : SKObject
    {
        internal readonly Dictionary<IntPtr, T> ManagedInstances = new Dictionary<IntPtr, T>();
        
        public virtual void DisposeObject(IntPtr objPtr)
        {
            ManagedInstances[objPtr].Dispose();
            ManagedInstances.Remove(objPtr);
        }
        
        public T this[IntPtr objPtr]
        {
            get => ManagedInstances[objPtr];
            set => ManagedInstances[objPtr] = value;
        }
    }
}
