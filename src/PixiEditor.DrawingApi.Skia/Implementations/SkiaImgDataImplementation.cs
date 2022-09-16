using System;
using System.Collections.Generic;
using System.IO;
using PixiEditor.DrawingApi.Core.Bridge.NativeObjectsImpl;
using PixiEditor.DrawingApi.Core.Surface.ImageData;
using SkiaSharp;

namespace PixiEditor.DrawingApi.Skia.Implementations
{
    public sealed class SkiaImgDataImplementation : IImgDataImplementation
    {
        internal readonly Dictionary<IntPtr, SKData> ManagedImgDataObjects = new Dictionary<IntPtr, SKData>();

        public void Dispose(IntPtr objectPointer)
        {
            if (ManagedImgDataObjects.ContainsKey(objectPointer))
            {
                ManagedImgDataObjects[objectPointer].Dispose();
                ManagedImgDataObjects.Remove(objectPointer);
            }
        }

        public void SaveTo(ImgData imgData, FileStream stream)
        {
            SKData data = ManagedImgDataObjects[imgData.ObjectPointer];
            data.SaveTo(stream);
        }

        public Stream AsStream(ImgData imgData)
        {
            SKData data = ManagedImgDataObjects[imgData.ObjectPointer];
            return data.AsStream();
        }
    }
}
