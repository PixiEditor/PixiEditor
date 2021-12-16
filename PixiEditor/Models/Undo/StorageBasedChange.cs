using PixiEditor.Models.DataHolders;
using PixiEditor.Models.IO;
using PixiEditor.Models.Layers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using PixiEditor.Models.ImageManipulation;
using PixiEditor.Models.Position;
using SkiaSharp;

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

        private List<Guid> layersToStore = new List<Guid>();
        public Document Document { get; }

        public StorageBasedChange(Document doc, IEnumerable<Layer> layers, bool saveOnStartup = true)
        {
            Document = doc;
            Initialize(layers, DefaultUndoChangeLocation, saveOnStartup);
        }

        public StorageBasedChange(Document doc, IEnumerable<Layer> layers, string undoChangeLocation, bool saveOnStartup = true)
        {
            Document = doc;
            Initialize(layers, undoChangeLocation, saveOnStartup);
        }

        public StorageBasedChange(Document doc, IEnumerable<LayerChunk> chunks, bool saveOnStartup = true)
        {
            Document = doc;
            var chunkData = chunks as LayerChunk[] ?? chunks.ToArray();
            LayerChunk[] layerChunks = new LayerChunk[chunkData.Length];
            for (var i = 0; i < chunkData.Length; i++)
            {
                var chunk = chunkData[i];
                layerChunks[i] = chunk;
                layersToStore.Add(chunk.Layer.LayerGuid);
            }

            UndoChangeLocation = DefaultUndoChangeLocation;
            GenerateUndoLayers(layerChunks);
            if (saveOnStartup)
            {
                SaveLayersOnDevice();
            }
        }

        private void Initialize(IEnumerable<Layer> layers, string undoChangeLocation, bool saveOnStartup)
        {
            var layersArray = layers as Layer[] ?? layers.ToArray();
            LayerChunk[] layerChunks = new LayerChunk[layersArray.Length];
            for (var i = 0; i < layersArray.Length; i++)
            {
                var layer = layersArray[i];
                layerChunks[i] = new LayerChunk(layer, SKRectI.Create(0, 0, layer.Width, layer.Height));
                layersToStore.Add(layer.LayerGuid);
            }

            UndoChangeLocation = undoChangeLocation;
            GenerateUndoLayers(layerChunks);
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
                Layer layer = Document.Layers.First(x => x.LayerGuid == layerGuid);
                UndoLayer storedLayer = StoredLayers[i];
                if (Directory.Exists(Path.GetDirectoryName(storedLayer.StoredPngLayerName)))
                {
                    // Calculate absolute rect to relative rect
                    SKRectI finalRect = SKRectI.Create(
                        storedLayer.SerializedRect.Left - layer.OffsetX,
                        storedLayer.SerializedRect.Top - layer.OffsetY,
                        storedLayer.SerializedRect.Width,
                        storedLayer.SerializedRect.Height);

                    using var image = layer.LayerBitmap.SkiaSurface.Snapshot();
                    Surface targetSizeSurface = new Surface(finalRect.Width, finalRect.Height);

                    targetSizeSurface.SkiaSurface.Canvas.DrawImage(image, finalRect, SKRect.Create(0, 0, finalRect.Width, finalRect.Height), Surface.ReplacingPaint);

                    Exporter.SaveAsGZippedBytes(storedLayer.StoredPngLayerName, targetSizeSurface);
                }

                i++;
            }

            layersToStore = new List<Guid>();
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
                var bitmap = Importer.LoadFromGZippedBytes(storedLayer.StoredPngLayerName);
                layers[i] = new Layer(storedLayer.Name, bitmap)
                {
                    Width = storedLayer.Width,
                    Height = storedLayer.Height,
                    Offset = new Thickness(storedLayer.OffsetX, storedLayer.OffsetY, 0, 0),
                    Opacity = storedLayer.Opacity,
                    MaxWidth = storedLayer.MaxWidth,
                    MaxHeight = storedLayer.MaxHeight,
                    IsVisible = storedLayer.IsVisible,
                    IsActive = storedLayer.IsActive,
                    LayerHighlightColor = storedLayer.LayerHighlightColor
                };

                layers[i].ChangeGuid(storedLayer.LayerGuid);

#if DEBUG
                File.Delete(StoredLayers[i].StoredPngLayerName + ".png");
#endif
                File.Delete(StoredLayers[i].StoredPngLayerName);
            }

            layersToStore = layers.Select(x => x.LayerGuid).ToList();
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
        ///     Creates UndoManager ready Change instance, where undo and redo is the same, before process images are loaded from disk and current ones are saved.
        /// </summary>
        /// <param name="undoRedoProcess">Process that is invoked on redo and undo.</param>
        /// <param name="processArgs">Custom parameters for undo and redo process.</param>
        /// <param name="description">Undo change description.</param>
        /// <returns>UndoManager ready 'Change' instance.</returns>
        public Change ToChange(Action<Layer[], UndoLayer[], object[]> undoRedoProcess, object[] processArgs, string description = "")
        {
            Action<object[]> finalProcess = processParameters =>
            {
                Layer[] layers = LoadLayersFromDevice();
                LayerChunk[] chunks = new LayerChunk[layers.Length];
                for (int i = 0; i < layers.Length; i++)
                {
                    chunks[i] = new LayerChunk(layers[i], StoredLayers[i].SerializedRect);
                }

                GenerateUndoLayers(chunks);

                SaveLayersOnDevice();

                undoRedoProcess(layers, StoredLayers, processParameters);
            };

            return new Change(finalProcess, processArgs, finalProcess, processArgs, description);
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
        private void GenerateUndoLayers(LayerChunk[] chunks)
        {
            StoredLayers = new UndoLayer[layersToStore.Count];
            int i = 0;
            foreach (var layerGuid in layersToStore)
            {
                Layer layer = Document.Layers.First(x => x.LayerGuid == layerGuid);
                if (!Document.Layers.Contains(layer))
                {
                    throw new ArgumentException("Provided document doesn't contain selected layer");
                }

                int index = Document.Layers.IndexOf(layer);
                string fileName = layer.Name + Guid.NewGuid();
                StoredLayers[i] = new UndoLayer(
                    Path.Join(
                        UndoChangeLocation,
                        Convert.ToBase64String(Encoding.UTF8.GetBytes(fileName)) + ".undoimg"),
                    layer,
                    index,
                    chunks[i].AbsoluteChunkRect);
                i++;
            }
        }

        public static void BasicUndoProcess(Layer[] layers, UndoLayer[] data, object[] args)
        {
            if (args.Length > 0 && args[0] is Document document)
            {
                for (int i = 0; i < layers.Length; i++)
                {
                    Layer layer = layers[i];
                    UndoLayer layerData = data[i];
                    var foundLayer = document.Layers.FirstOrDefault(x => x.LayerGuid == layerData.LayerGuid);

                    if (foundLayer != null)
                    {
                        ApplyChunkToLayer(foundLayer, layerData, layer.LayerBitmap);
                    }
                    else
                    {
                        document.RemoveLayer(layerData.LayerIndex, false);
                        document.Layers.Insert(layerData.LayerIndex, layer);
                    }

                    if (layerData.IsActive)
                    {
                        document.SetMainActiveLayer(layerData.LayerIndex);
                    }
                }
            }
        }

        private static void ApplyChunkToLayer(Layer layer, UndoLayer layerData, Surface chunk)
        {
            Surface targetSizeSurface = new Surface(layerData.Width, layerData.Height);
            using var foundLayerSnapshot = layer.LayerBitmap.SkiaSurface.Snapshot();
            targetSizeSurface.SkiaSurface.Canvas.DrawImage(
                foundLayerSnapshot,
                SKRect.Create(layerData.OffsetX - layer.OffsetX, layerData.OffsetY - layer.OffsetY, layerData.Width, layerData.Height),
                SKRect.Create(0, 0, layerData.Width, layerData.Height),
                Surface.ReplacingPaint);

            layer.Offset = new Thickness(layerData.OffsetX, layerData.OffsetY, 0, 0);

            SKRect finalRect = SKRect.Create(
                layerData.SerializedRect.Left - layer.OffsetX,
                layerData.SerializedRect.Top - layer.OffsetY,
                layerData.SerializedRect.Width,
                layerData.SerializedRect.Height);

            using var snapshot = chunk.SkiaSurface.Snapshot();

            targetSizeSurface.SkiaSurface.Canvas.DrawImage(
                snapshot,
                finalRect,
                Surface.ReplacingPaint);

            layer.LayerBitmap = targetSizeSurface;
        }
    }
}
