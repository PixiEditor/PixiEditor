using System.Diagnostics;
using System.IO;
using System.Windows.Media.Imaging;
using PixiEditor.Helpers;
using PixiEditor.Models.ImageManipulation;
using PixiEditor.Models.IO;
using PixiEditor.Parser;

namespace PixiEditor.Models.DataHolders
{
    [DebuggerDisplay("{FilePath}")]
    public class RecentlyOpenedDocument : NotifyableObject
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
                if (Corrupt)
                {
                    return "? (Corrupt)";
                }

                string extension = Path.GetExtension(filePath).ToLower();
                return extension is not (".pixi" or ".png" or ".jpg" or ".jpeg")
                    ? $"? ({extension})"
                    : extension;
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public WriteableBitmap PreviewBitmap
        {
            get
            {
                if (previewBitmap == null)
                {
                    PreviewBitmap = LoadPreviewBitmap();
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
            if (FileExtension == ".pixi")
            {
                SerializableDocument serializableDocument;

                try
                {
                    serializableDocument = PixiParser.Deserialize(filePath);
                }
                catch
                {
                    corrupt = true;
                    return null;
                }

                return PixiFileMaxSizeChecker.IsFileUnderMaxSize(serializableDocument) ?
                    BitmapUtils.GeneratePreviewBitmap(serializableDocument.Layers, serializableDocument.Width, serializableDocument.Height, 80, 50)
                    : null;
            }
            else if (FileExtension is ".png" or ".jpg" or ".jpeg")
            {
                WriteableBitmap bitmap = null;

                try
                {
                    bitmap = Importer.ImportImage(FilePath);
                }
                catch
                {
                    corrupt = true;
                }

                const int MaxWidthInPixels = 2048;
                const int MaxHeightInPixels = 2048;
                ImageFileMaxSizeChecker imageFileMaxSizeChecker = new ImageFileMaxSizeChecker(maxPixelCountAllowed: MaxWidthInPixels * MaxHeightInPixels);

                return imageFileMaxSizeChecker.IsFileUnderMaxSize(bitmap) ?
                    bitmap
                    : bitmap.Resize(width: MaxWidthInPixels, height: MaxHeightInPixels, WriteableBitmapExtensions.Interpolation.Bilinear);
            }

            return null;
        }
    }
}