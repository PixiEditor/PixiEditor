using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.IO;
using PixiEditor.Models.Layers;

namespace PixiEditor.Models.Undo
{
    public class StorageBasedChange
    {
        public string UndoChangeLocation { get; set; }

        public UndoLayer[] StoredLayers { get; set; }

        private IEnumerable<Layer> layersToStore;

        private Document document;

        public StorageBasedChange(Document doc, IEnumerable<Layer> layers, string undoChangeLocation)
        {
            document = doc;
            layersToStore = layers;
            UndoChangeLocation = undoChangeLocation;
            GenerateUndoLayers();
        }

        public void SaveLayersOnDevice()
        {
            int i = 0;
            foreach (var layer in layersToStore)
            {
                UndoLayer storedLayer = StoredLayers[i];
                Exporter.SaveAsPng(storedLayer.StoredPngLayerName, storedLayer.Width, storedLayer.Height, layer.LayerBitmap);
                i++;
            }

            layersToStore = null;
        }

        public Layer[] LoadLayersFromDevice()
        {
            Layer[] layers = new Layer[StoredLayers.Length];
            for (int i = 0; i < StoredLayers.Length; i++)
            {
                UndoLayer storedLayer = StoredLayers[i];
                var bitmap = Importer.ImportImage(storedLayer.StoredPngLayerName, storedLayer.Width, storedLayer.Height);
                layers[i] = new Layer(storedLayer.Name, bitmap)
                {
                    Offset = new System.Windows.Thickness(storedLayer.OffsetX, storedLayer.OffsetY, 0, 0),
                    Opacity = storedLayer.Opacity,
                    MaxWidth = storedLayer.MaxWidth,
                    MaxHeight = storedLayer.MaxHeight,
                    IsVisible = storedLayer.IsVisible,
                    IsActive = storedLayer.IsActive,
                };

                File.Delete(StoredLayers[i].StoredPngLayerName);
            }

            layersToStore = layers;
            return layers;
        }

        private void GenerateUndoLayers()
        {
            StoredLayers = new UndoLayer[layersToStore.Count()];
            int i = 0;
            foreach (var layer in layersToStore)
            {
                if (!document.Layers.Contains(layer))
                {
                    throw new ArgumentException("Provided document doesn't contain selected layer");
                }

                int index = document.Layers.IndexOf(layer);
                string pngName = layer.Name + index + Guid.NewGuid().ToString();
                StoredLayers[i] = new UndoLayer(
                    Path.Join(
                        UndoChangeLocation,
                        Convert.ToBase64String(Encoding.UTF8.GetBytes(pngName)) + ".png"),
                    layer,
                    index);
                i++;
            }
        }
    }
}