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
    /// <summary>
    ///     A class that allows to save layers on disk and load them on Undo/Redo.
    /// </summary>
    public class StorageBasedChange
    {
        public static string DefaultUndoChangeLocation => Path.Join(Path.GetTempPath(), "PixiEditor", "UndoStack");

        public string UndoChangeLocation { get; set; }

        public UndoLayer[] StoredLayers { get; set; }

        private IEnumerable<Guid> layersToStore;

        private Document document;

        public StorageBasedChange(Document doc, IEnumerable<Layer> layers, bool saveOnStartup = true)
        {
            document = doc;
            layersToStore = layers.Select(x => x.LayerGuid);
            UndoChangeLocation = DefaultUndoChangeLocation;
            GenerateUndoLayers();
            if (saveOnStartup)
            {
                SaveLayersOnDevice();
            }
        }

        public StorageBasedChange(Document doc, IEnumerable<Layer> layers, string undoChangeLocation, bool saveOnStartup = true)
        {
            document = doc;
            layersToStore = layers.Select(x => x.LayerGuid);
            UndoChangeLocation = undoChangeLocation;
            GenerateUndoLayers();

            if (saveOnStartup)
            {
                SaveLayersOnDevice();
            }
        }

        public void SaveLayersOnDevice()
        {
            int i = 0;
            foreach (var layerGuid in layersToStore)
            {
                Layer layer = document.Layers.First(x => x.LayerGuid == layerGuid);
                UndoLayer storedLayer = StoredLayers[i];
                if (Directory.Exists(Path.GetDirectoryName(storedLayer.StoredPngLayerName)))
                {
                    Exporter.SaveAsImage(storedLayer.StoredPngLayerName, storedLayer.Width, storedLayer.Height, layer.LayerBitmap);
                }

                i++;
            }

            layersToStore = Array.Empty<Guid>();
        }

        /// <summary>
        /// Loads saved layers from disk.
        /// </summary>
        /// <returns>Array of saved layers.</returns>
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
                    Width = storedLayer.Width,
                    Height = storedLayer.Height,
                    LayerHighlightColor = storedLayer.LayerHighlightColor                    
                };
                layers[i].ChangeGuid(storedLayer.LayerGuid);

                File.Delete(StoredLayers[i].StoredPngLayerName);
            }

            layersToStore = layers.Select(x => x.LayerGuid);
            return layers;
        }

        /// <summary>
        ///     Creates UndoManager ready Change instance, where undo process loads layers from device, and redo saves them.
        /// </summary>
        /// <param name="undoProcess">Method that is invoked on undo, with loaded layers parameter and UndoLayer array data.</param>
        /// <param name="processArgs">Custom parameters for undo process.</param>
        /// <param name="redoProcess">Method that is invoked on redo with custom object array parameters.</param>
        /// <param name="redoProcessParameters">Parameters for redo process.</param>
        /// <param name="description">Undo change description.</param>
        /// <returns>UndoManager ready Change instance.</returns>
        public Change ToChange(Action<Layer[], UndoLayer[], object[]> undoProcess, object[] processArgs, Action<object[]> redoProcess, object[] redoProcessParameters, string description = "")
        {
            Action<object[]> finalUndoProcess = processParameters =>
            {
                Layer[] layers = LoadLayersFromDevice();
                undoProcess(layers, StoredLayers, processParameters);
            };

            Action<object[]> finalRedoProcess = parameters =>
            {
                SaveLayersOnDevice();
                redoProcess(parameters);
            };

            return new Change(finalUndoProcess, processArgs, finalRedoProcess, redoProcessParameters, description);
        }

        /// <summary>
        ///     Creates UndoManager ready Change instance, where undo process loads layers from device, and redo saves them.
        /// </summary>
        /// <param name="undoProcess">Method that is invoked on undo, with loaded layers parameter and UndoLayer array data.</param>
        /// <param name="redoProcess">Method that is invoked on redo with custom object array parameters.</param>
        /// <param name="redoProcessParameters">Parameters for redo process.</param>
        /// <param name="description">Undo change description.</param>
        /// <returns>UndoManager ready Change instance.</returns>
        public Change ToChange(Action<Layer[], UndoLayer[]> undoProcess, Action<object[]> redoProcess, object[] redoProcessParameters, string description = "")
        {
            Action<object[]> finalUndoProcess = _ =>
            {
                Layer[] layers = LoadLayersFromDevice();
                undoProcess(layers, StoredLayers);
            };

            Action<object[]> finalRedoProcess = parameters =>
            {
                SaveLayersOnDevice();
                redoProcess(parameters);
            };

            return new Change(finalUndoProcess, null, finalRedoProcess, redoProcessParameters, description);
        }

        /// <summary>
        ///     Creates UndoManager ready Change instance, where undo process saves layers on device, and redo loads them.
        /// </summary>
        /// <param name="undoProcess">Method that is invoked on undo, with loaded layers parameter and UndoLayer array data.</param>
        /// <param name="undoProcessParameters">Parameters for undo process.</param>
        /// <param name="redoProcess">Method that is invoked on redo with custom object array parameters.</param>
        /// <param name="description">Undo change description.</param>
        /// <returns>UndoManager ready Change instance.</returns>
        public Change ToChange(Action<object[]> undoProcess, object[] undoProcessParameters, Action<Layer[], UndoLayer[]> redoProcess, string description = "")
        {
            Action<object[]> finalUndoProcess = parameters =>
            {
                SaveLayersOnDevice();
                undoProcess(parameters);
            };

            Action<object[]> finalRedoProcess = parameters =>
            {
                Layer[] layers = LoadLayersFromDevice();
                redoProcess(layers, StoredLayers);
            };

            return new Change(finalUndoProcess, undoProcessParameters, finalRedoProcess, null, description);
        }

        /// <summary>
        ///     Creates UndoManager ready Change instance, where undo process saves layers on device, and redo loads them.
        /// </summary>
        /// <param name="undoProcess">Method that is invoked on undo, with loaded layers parameter and UndoLayer array data.</param>
        /// <param name="undoProcessParameters">Parameters for undo process.</param>
        /// <param name="redoProcess">Method that is invoked on redo with custom object array parameters.</param>
        /// <param name="redoProcessArgs">Parameters for redo process.</param>
        /// <param name="description">Undo change description.</param>
        /// <returns>UndoManager ready Change instance.</returns>
        public Change ToChange(Action<object[]> undoProcess, object[] undoProcessParameters, Action<Layer[], UndoLayer[], object[]> redoProcess, object[] redoProcessArgs, string description = "")
        {
            Action<object[]> finalUndoProcess = parameters =>
            {
                SaveLayersOnDevice();
                undoProcess(parameters);
            };

            Action<object[]> finalRedoProcess = parameters =>
            {
                Layer[] layers = LoadLayersFromDevice();
                redoProcess(layers, StoredLayers, parameters);
            };

            return new Change(finalUndoProcess, undoProcessParameters, finalRedoProcess, redoProcessArgs, description);
        }

        /// <summary>
        /// Generates UndoLayer[] StoredLayers data.
        /// </summary>
        private void GenerateUndoLayers()
        {
            StoredLayers = new UndoLayer[layersToStore.Count()];
            int i = 0;
            foreach (var layerGuid in layersToStore)
            {
                Layer layer = document.Layers.First(x => x.LayerGuid == layerGuid);
                if (!document.Layers.Contains(layer))
                {
                    throw new ArgumentException("Provided document doesn't contain selected layer");
                }

                layer.ClipCanvas();

                int index = document.Layers.IndexOf(layer);
                string pngName = layer.Name + Guid.NewGuid().ToString();
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