using PixiEditor.Models.Controllers;
using PixiEditor.Models.ImageManipulation;
using PixiEditor.Models.Layers;
using System.Windows.Media.Imaging;

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
        private SingleLayerRenderer previewLayerRenderer;

        public Layer PreviewLayer
        {
            get => previewLayer;
            set
            {
                previewLayer = value;
                previewLayerRenderer?.Dispose();
                previewLayerRenderer = previewLayer == null ? null : new SingleLayerRenderer(previewLayer, Width, Height);
                RaisePropertyChanged(nameof(PreviewLayer));
                RaisePropertyChanged(nameof(PreviewLayerRenderer));
            }
        }

        public SingleLayerRenderer PreviewLayerRenderer
        {
            get => previewLayerRenderer;
        }

        public void UpdatePreviewImage()
        {
            previewImage = BitmapUtils.GeneratePreviewBitmap(this, 30, 20);
            RaisePropertyChanged(nameof(PreviewImage));
        }

        public void GeneratePreviewLayer()
        {
            PreviewLayer = new Layer("_previewLayer", Width, Height);
        }
    }
}
