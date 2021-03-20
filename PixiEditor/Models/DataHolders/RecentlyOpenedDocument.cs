using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using PixiEditor.Helpers;
using PixiEditor.Models.ImageManipulation;
using PixiEditor.Models.IO;
using PixiEditor.Models.Layers;
using PixiEditor.Parser;

namespace PixiEditor.Models.DataHolders
{
    [DebuggerDisplay("{FilePath}")]
    public class RecentlyOpenedDocument : NotifyableObject
    {
        private string filePath;

        private WriteableBitmap previewBitmap;

        public string FilePath
        {
            get => filePath;
            set
            {
                SetProperty(ref filePath, value);
                RaisePropertyChanged(nameof(FileName));
                PreviewBitmap = null;
            }
        }

        public string FileName => Path.GetFileName(filePath);

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
            if (FilePath.EndsWith(".pixi"))
            {
                SerializableDocument serializableDocument = PixiParser.Deserialize(filePath);

                return BitmapUtils.GeneratePreviewBitmap(serializableDocument.Layers, serializableDocument.Width, serializableDocument.Height, 80, 50);
            }
            else
            {
                WriteableBitmap bitmap = Importer.ImportImage(FilePath);

                return bitmap;
            }
        }
    }
}