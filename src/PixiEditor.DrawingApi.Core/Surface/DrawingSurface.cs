﻿using System;
using PixiEditor.DrawingApi.Core.Bridge;
using PixiEditor.DrawingApi.Core.Surface.ImageData;

namespace PixiEditor.DrawingApi.Core.Surface
{
    public class DrawingSurface
    {
        public float Width { get; set; }
        public float Height { get; set; }
        
        public DrawingSurfaceProperties Properties { get; private set; }
        public Canvas Canvas { get; private set; }

        public Image Snapshot()
        {
            return DrawingBackendApi.Current.ImageOperations.Snapshot(this);
        }

        public Pixmap PeekPixels()
        {
            return DrawingBackendApi.Current.ImageOperations.PeekPixels(this);
        }

        public static DrawingSurface Create(ImageInfo imageInfo, IntPtr arr, int chunkSize)
        {
            
        }
    }
}