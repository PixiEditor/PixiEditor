using System;
using PixiEditor.DrawingApi.Core.Bridge;
using PixiEditor.DrawingApi.Core.Shaders;
using PixiEditor.Numerics;

namespace PixiEditor.DrawingApi.Core.Surfaces.ImageData
{
    /// <summary>An abstraction for drawing a rectangle of pixels.</summary>
    /// <remarks>
    ///     <para>An image is an abstraction of pixels, though the particular type of image could be actually storing its data on the GPU, or as drawing commands (picture or PDF or otherwise), ready to be played back into another canvas.</para>
    ///     <para />
    ///     <para>The content of an image is always immutable, though the actual storage may change, if for example that image can be recreated via encoded data or other means.</para>
    ///     <para />
    ///     <para>An image always has a non-zero dimensions. If there is a request to create a new image, either directly or via a surface, and either of the requested dimensions are zero, then <see langword="null" /> will be returned.</para>
    /// </remarks>
    public class Image : NativeObject, ICloneable, IPixelsMap
    {
        public override object Native => DrawingBackendApi.Current.ImageImplementation.GetNativeImage(ObjectPointer);

        public int Width => DrawingBackendApi.Current.ImageImplementation.GetWidth(ObjectPointer);
        
        public int Height => DrawingBackendApi.Current.ImageImplementation.GetHeight(ObjectPointer);
        
        public ImageInfo Info => DrawingBackendApi.Current.ImageImplementation.GetImageInfo(ObjectPointer);
        
        public VecI Size => new VecI(Width, Height);
        
        public Image(IntPtr objPtr) : base(objPtr)
        {
        }

        public override void Dispose()
        {
            DrawingBackendApi.Current.ImageImplementation.DisposeImage(this);
        }

        public static Image? FromEncodedData(string path)
        {
            return DrawingBackendApi.Current.ImageImplementation.FromEncodedData(path);
        }
        
        public static Image? FromEncodedData(byte[] dataBytes)
        {
            return DrawingBackendApi.Current.ImageImplementation.FromEncodedData(dataBytes);
        }

        public static Image? FromPixels(ImageInfo info, byte[] pixels)
        {
            return DrawingBackendApi.Current.ImageImplementation.FromPixelCopy(info, pixels);
        }

        public ImgData Encode()
        {
            return DrawingBackendApi.Current.ImageImplementation.Encode(this);
        }

        public ImgData Encode(EncodedImageFormat format, int quality = 100)
        {
            return DrawingBackendApi.Current.ImageImplementation.Encode(this, format, quality);
        }

        public Pixmap PeekPixels()
        {
            return DrawingBackendApi.Current.ImageImplementation.PeekPixels(ObjectPointer);
        }

        public object Clone()
        {
            return DrawingBackendApi.Current.ImageImplementation.Clone(this);
        }

        public Shader ToShader()
        {
            return DrawingBackendApi.Current.ImageImplementation.ToShader(ObjectPointer);
        }
    }
}
