using PixiEditor.Models.DataHolders;
using PixiEditor.Models.IO;
using PixiEditor.Models.Layers;

namespace PixiEditor.Models.Undo
{
    public class StorageBasedChange
    {
        public string UndoChangeLocation { get; set; }

        public void SaveLayersOnDevice(Document document, Layer[] layers)
        {
            Exporter.SaveAsPng(UndoChangeLocation, layers[0].Width, layers[0].Height, layers[0].LayerBitmap)
        }
    }
}