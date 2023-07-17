using System.Diagnostics;
using System.IO;
using System.Linq;
using ChunkyImageLib;
using PixiEditor.Avalonia.Exceptions.Exceptions;
using PixiEditor.Parser.Deprecated;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Helpers;
using PixiEditor.Parser;
using ReactiveUI;

namespace PixiEditor.Models.DataHolders;

[DebuggerDisplay("{FilePath}")]
internal class RecentlyOpenedDocument : ReactiveObject
{
    private bool corrupt;

    private string filePath;

    private SKBitmap previewBitmap;

    public string FilePath
    {
        get => filePath;
        set
        {
            this.RaiseAndSetIfChanged(ref filePath, value);
            this.RaisePropertyChanged(nameof(FileName));
            this.RaisePropertyChanged(nameof(FileExtension));
            PreviewBitmap = null;
        }
    }

    public bool Corrupt
    {
        get => corrupt;
        set => this.RaiseAndSetIfChanged(ref corrupt, value);
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

    public SKBitmap PreviewBitmap
    {
        get
        {
            if (previewBitmap == null && !Corrupt)
            {
                previewBitmap = LoadPreviewBitmap();
            }

            return previewBitmap;
        }
        private set => this.RaiseAndSetIfChanged(ref previewBitmap, value);
    }

    public RecentlyOpenedDocument(string path)
    {
        FilePath = path;
    }

    private SKBitmap LoadPreviewBitmap()
    {
        if (!File.Exists(FilePath))
        {
            return null;
        }

        if (FileExtension == ".pixi")
        {
            SerializableDocument serializableDocument;

            try
            {
                var document = PixiParser.Deserialize(filePath);

                if (document.PreviewImage == null || document.PreviewImage.Length == 0)
                {
                    return null;
                }

                using var data = new MemoryStream(document.PreviewImage);
                return SKBitmap.Decode(data);
            }
            catch
            {

                try
                {
                    serializableDocument = DepractedPixiParser.Deserialize(filePath);
                }
                catch
                {
                    corrupt = true;
                    return null;
                }
            }

            //TODO: Fix this
            /*using Surface surface = Surface.Combine(serializableDocument.Width, serializableDocument.Height,
                serializableDocument.Layers
                    .Where(x => x.Opacity > 0.8)
                    .Select(x => (x.ToImage(), new VecI(x.OffsetX, x.OffsetY))).ToList());

            return DownscaleToMaxSize(surface.ToBitmap());*/

            return null;
        }

        if (SupportedFilesHelper.IsExtensionSupported(FileExtension))
        {
            SKBitmap bitmap = null;

            try
            {
                //TODO: Fix this
                //bitmap = Importer.ImportWriteableBitmap(FilePath);
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

    private SKBitmap DownscaleToMaxSize(SKBitmap bitmap)
    {
        if (bitmap.Width > Constants.MaxPreviewWidth || bitmap.Height > Constants.MaxPreviewHeight)
        {
            double factor = Math.Min(Constants.MaxPreviewWidth / (double)bitmap.Width, Constants.MaxPreviewHeight / (double)bitmap.Height);
            return bitmap.Resize(new SKSizeI((int)(bitmap.Width * factor), (int)(bitmap.Height * factor)), SKFilterQuality.High);
        }

        return bitmap;
    }
}
