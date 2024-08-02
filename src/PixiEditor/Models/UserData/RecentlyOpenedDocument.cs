using System.Diagnostics;
using System.IO;
using System.Linq;
using ChunkyImageLib;
using CommunityToolkit.Mvvm.ComponentModel;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Extensions.Exceptions;
using PixiEditor.Helpers;
using PixiEditor.Models.IO;
using PixiEditor.Numerics;
using PixiEditor.Parser;

namespace PixiEditor.Models.UserData;

[DebuggerDisplay("{FilePath}")]
internal class RecentlyOpenedDocument : ObservableObject
{
    private bool corrupt;

    private string filePath;

    private Texture previewBitmap;

    public string FilePath
    {
        get => filePath;
        set
        {
            SetProperty(ref filePath, value);
            this.OnPropertyChanged(nameof(FileName));
            this.OnPropertyChanged(nameof(FileExtension));
            PreviewBitmap = null;
        }
    }

    public bool Corrupt
    {
        get => corrupt;
        set => SetProperty(ref corrupt, value);
    }

    public string FileName => Path.GetFileNameWithoutExtension(filePath);

    public string FileExtension
    {
        get
        {
            if (!File.Exists(FilePath))
            {
                return "? (Not found)";
            }

            if (Corrupt)
            {
                return "? (Corrupt)";
            }

            string extension = Path.GetExtension(filePath).ToLower();
            return SupportedFilesHelper.IsExtensionSupported(extension) ? extension : $"? ({extension})";
        }
    }

    public Texture PreviewBitmap
    {
        get
        {
            if (previewBitmap == null && !Corrupt)
            {
                previewBitmap = LoadPreviewBitmap();
            }

            return previewBitmap;
        }
        private set => SetProperty(ref previewBitmap, value);
    }

    public RecentlyOpenedDocument(string path)
    {
        FilePath = path;
    }

    private Texture? LoadPreviewBitmap()
    {
        if (!File.Exists(FilePath))
        {
            return null;
        }

        if (FileExtension == ".pixi")
        {
            try
            {
                return Importer.GetPreviewTexture(FilePath);
            }
            catch
            {
                return null;
            }
        }

        if (SupportedFilesHelper.IsExtensionSupported(FileExtension))
        {
            Texture bitmap = null;

            try
            {
                bitmap = Texture.Load(FilePath);
            }
            catch (RecoverableException)
            {
                corrupt = true;
                return null;
            }

            if (bitmap == null) //prevent crash
                return null;

            return DownscaleToMaxSize(bitmap);
        }

        return null;
    }

    private Texture DownscaleToMaxSize(Texture bitmap)
    {
        if (bitmap.Size.X > Constants.MaxPreviewWidth || bitmap.Size.Y > Constants.MaxPreviewHeight)
        {
            double factor = Math.Min(Constants.MaxPreviewWidth / (double)bitmap.Size.X, Constants.MaxPreviewHeight / (double)bitmap.Size.Y);
            var scaledBitmap = bitmap.CreateResized(new VecI((int)(bitmap.Size.X * factor), (int)(bitmap.Size.Y * factor)),
                ResizeMethod.HighQuality);
            return scaledBitmap;
        }

        return bitmap;
    }
}
