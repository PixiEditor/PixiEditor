using System.Windows.Media.Imaging;
using PixiEditor.Models.ImageManipulation;
using PixiEditor.Models.Layers;

namespace PixiEditor.Models.DataHolders
{
    public partial class Document
    {
        private WriteableBitmap previewImage;

        public WriteableBitmap PreviewImage
        {
            get => previewImage;
        }

        private Layer previewLayer;

        public Layer PreviewLayer
        {
            get => previewLayer;
            set
            {
                previewLayer = value;
                RaisePropertyChanged(nameof(PreviewLayer));
            }
        }

        public void UpdatePreviewImage()
        {
            previewImage = BitmapUtils.GeneratePreviewBitmap(this, 30, 20);
            RaisePropertyChanged(nameof(PreviewImage));
        }

        public void GeneratePreviewLayer()
        {
            PreviewLayer = new Layer("_previewLayer")
            {
                MaxWidth = Width,
                MaxHeight = Height
            };
        }
    }
}