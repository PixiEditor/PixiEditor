using System.Drawing;
using Avalonia;
using Avalonia.Threading;
using FFMpegCore.Enums;
using PixiEditor.DrawingApi.Core;
using PixiEditor.Extensions.Exceptions;
using PixiEditor.Helpers;
using PixiEditor.Models;
using PixiEditor.Models.IO;
using PixiEditor.Numerics;
using PixiEditor.Parser;
using Image = Avalonia.Controls.Image;

namespace PixiEditor.Views.Visuals;

internal class PixiFilePreviewImage : SurfaceControl
{
    public static readonly StyledProperty<string> FilePathProperty =
        AvaloniaProperty.Register<PixiFilePreviewImage, string>(nameof(FilePath));

    public static readonly StyledProperty<VecI> ImageSizeProperty =
        AvaloniaProperty.Register<PixiFilePreviewImage, VecI>(nameof(VecI));

    public static readonly StyledProperty<bool> CorruptProperty =
        AvaloniaProperty.Register<PixiFilePreviewImage, bool>(nameof(Corrupt));

    public string FilePath
    {
        get => GetValue(FilePathProperty);
        set => SetValue(FilePathProperty, value);
    }
    
    public VecI ImageSize
    {
        get => GetValue(ImageSizeProperty);
        set => SetValue(ImageSizeProperty, value);
    }

    public bool Corrupt
    {
        get => GetValue(CorruptProperty);
        set => SetValue(CorruptProperty, value);
    }

    static PixiFilePreviewImage()
    {
        FilePathProperty.Changed.AddClassHandler<PixiFilePreviewImage>(OnFilePathChanged);
    }

    private void RunLoadImage()
    {
        var path = FilePath;

        Task.Run(() => LoadImage(path));
    }

    private void LoadImage(string path)
    {
        var surface = LoadPreviewSurface(path);
        
        Dispatcher.UIThread.Post(() => SetImage(surface));
    }

    private void SetImage(Surface? surface)
    {
        Surface = surface!;

        if (surface != null)
        {
            ImageSize = surface.Size;
        }
    }

    private static void OnFilePathChanged(PixiFilePreviewImage previewImage, AvaloniaPropertyChangedEventArgs args)
    {
        if (args.NewValue == null)
        {
            previewImage.Surface = null;
            return;
        }
        
        previewImage.RunLoadImage();
    }
    
    private Surface? LoadPreviewSurface(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return null;
        }
        
        var fileExtension = Path.GetExtension(filePath);

        if (fileExtension == ".pixi")
        {
            try
            {
                var result = Importer.GetPreviewSurface(filePath);

                if (result == null)
                {
                    SetCorrupt();
                }
            }
            catch
            {
                SetCorrupt();
                return null;
            }
        }

        if (SupportedFilesHelper.IsExtensionSupported(fileExtension))
        {
            Surface bitmap = null;

            try
            {
                bitmap = Surface.Load(filePath);
            }
            catch (RecoverableException)
            {
                SetCorrupt();
            }

            if (bitmap == null) //prevent crash
                return null;

            return DownscaleToMaxSize(bitmap);
        }

        return null;

        // TODO: This does not actually set the dot to gray
        void SetCorrupt()
        {
            Dispatcher.UIThread.Post(() => Corrupt = true);
        }
    }

    private static Surface DownscaleToMaxSize(Surface bitmap)
    {
        if (bitmap.Size.X > Constants.MaxPreviewWidth || bitmap.Size.Y > Constants.MaxPreviewHeight)
        {
            double factor = Math.Min(Constants.MaxPreviewWidth / (double)bitmap.Size.X, Constants.MaxPreviewHeight / (double)bitmap.Size.Y);
            var scaledBitmap = bitmap.Resize(new VecI((int)(bitmap.Size.X * factor), (int)(bitmap.Size.Y * factor)), ResizeMethod.HighQuality);
            
            bitmap.Dispose();
            return scaledBitmap;
        }
    
        return bitmap;
    }
    
}
