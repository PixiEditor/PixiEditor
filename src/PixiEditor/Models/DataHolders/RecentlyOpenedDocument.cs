using System.Diagnostics;
using System.IO;
using System.Windows.Media.Imaging;
using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using PixiEditor.Parser.Deprecated;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Helpers;
using PixiEditor.Models.IO;
using PixiEditor.Parser;
using PixiEditor.Parser.Skia;
using PixiEditor.Exceptions;
using PixiEditor.Numerics;

namespace PixiEditor.Models.DataHolders;

[DebuggerDisplay("{FilePath}")]
internal class RecentlyOpenedDocument : NotifyableObject
{
    private bool corrupt;

    private string filePath;

    private WriteableBitmap previewBitmap;

    public string FilePath
    {
        get => filePath;
        set
        {
            SetProperty(ref filePath, value);
            RaisePropertyChanged(nameof(FileName));
            RaisePropertyChanged(nameof(FileExtension));
            PreviewBitmap = null;
        }
    }

    public bool Corrupt { get => corrupt; set => SetProperty(ref corrupt, value); }

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

    public WriteableBitmap PreviewBitmap
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

    private WriteableBitmap LoadPreviewBitmap()
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
                var decoder = new PngBitmapDecoder(data, BitmapCreateOptions.None, BitmapCacheOption.OnDemand);
                return new WriteableBitmap(decoder.Frames[0]);
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

            using Surface surface = Surface.Combine(serializableDocument.Width, serializableDocument.Height,
                serializableDocument.Layers
                    .Where(x => x.Opacity > 0.8)
                    .Select(x => (x.ToImage(), new VecI(x.OffsetX, x.OffsetY))).ToList());

            return DownscaleToMaxSize(surface.ToWriteableBitmap());
        }
        if (SupportedFilesHelper.IsExtensionSupported(FileExtension))
        {
            WriteableBitmap bitmap = null;

            try
            {
                bitmap = Importer.ImportWriteableBitmap(FilePath);
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

    private WriteableBitmap DownscaleToMaxSize(WriteableBitmap bitmap)
    {
        if (bitmap.PixelWidth > Constants.MaxPreviewWidth || bitmap.PixelHeight > Constants.MaxPreviewHeight)
        {
            double factor = Math.Min(Constants.MaxPreviewWidth / (double)bitmap.PixelWidth, Constants.MaxPreviewHeight / (double)bitmap.PixelHeight);
            return bitmap.Resize((int)(bitmap.PixelWidth * factor), (int)(bitmap.PixelHeight * factor), WriteableBitmapExtensions.Interpolation.Bilinear);
        }
        return bitmap;
    }
}
