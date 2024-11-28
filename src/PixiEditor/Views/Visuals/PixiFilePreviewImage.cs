using System.Drawing;
using Avalonia;
using Avalonia.Threading;
using FFMpegCore.Enums;
using Drawie.Backend.Core;
using PixiEditor.Extensions.Exceptions;
using PixiEditor.Helpers;
using PixiEditor.Models;
using PixiEditor.Models.IO;
using Drawie.Numerics;
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
            return LoadPixiPreview(filePath);
        }

        if (SupportedFilesHelper.IsExtensionSupported(fileExtension))
        {
            return LoadNonPixiPreview(filePath);
        }

        return null;

    }

    private Surface LoadPixiPreview(string filePath)
    {
        try
        {
            var loaded = Importer.GetPreviewSurface(filePath);

            if (loaded.Size is { X: <= Constants.MaxPreviewWidth, Y: <= Constants.MaxPreviewHeight })
            {
                return loaded;
            }

            var downscaled = DownscaleSurface(loaded);
            loaded.Dispose();
            return downscaled;
        }
        catch
        {
            SetCorrupt();
            return null;
        }
    }

    private Surface LoadNonPixiPreview(string filePath)
    {
        Surface loaded = null;

        try
        {
            loaded = Surface.Load(filePath);
        }
        catch (RecoverableException)
        {
            SetCorrupt();
        }

        if (loaded == null) //prevent crash
            return null;

        if (loaded.Size is { X: <= Constants.MaxPreviewWidth, Y: <= Constants.MaxPreviewHeight })
        {
            return loaded;
        }

        var downscaled = DownscaleSurface(loaded);
        loaded.Dispose();
        return downscaled;

    }

    private static Surface DownscaleSurface(Surface surface)
    {
        double factor = Math.Min(
            Constants.MaxPreviewWidth / (double)surface.Size.X,
            Constants.MaxPreviewHeight / (double)surface.Size.Y);

        var newSize = new VecI((int)(surface.Size.X * factor), (int)(surface.Size.Y * factor));
        
        var scaledBitmap = surface.Resize(newSize, ResizeMethod.HighQuality);

        surface.Dispose();
        return scaledBitmap;
    }
    
    // TODO: This does not actually set the dot to gray
    void SetCorrupt()
    {
        Dispatcher.UIThread.Post(() => Corrupt = true);
    }
}
