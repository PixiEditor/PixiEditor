﻿using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace PixiEditor.AvaloniaUI.Helpers.Extensions;

public static class BitmapExtensions
{
    public static byte[] ExtractPixels(this Bitmap source)
    {
        return ExtractPixels(source, out _);
    }

    public static byte[] ExtractPixels(this Bitmap source, out IntPtr address)
    {
        var size = source.PixelSize;
        var stride = size.Width * 4;
        int bufferSize = stride * size.Height;

        byte[] target = new byte[bufferSize];

        address = Marshal.UnsafeAddrOfPinnedArrayElement(target, 0);

        source.CopyPixels(new PixelRect(0, 0, size.Width, size.Height), address, bufferSize, stride);
        return target;
    }

    public static WriteableBitmap ToWriteableBitmap(this Bitmap source)
    {
        var size = source.PixelSize;
        var stride = size.Width * 4;
        int bufferSize = stride * size.Height;

        byte[] target = new byte[bufferSize];

        var address = Marshal.UnsafeAddrOfPinnedArrayElement(target, 0);

        source.CopyPixels(new PixelRect(0, 0, size.Width, size.Height), address, bufferSize, stride);
        return new WriteableBitmap(PixelFormat.Bgra8888, AlphaFormat.Premul, address, size, new Vector(96, 96), stride);
    }
}