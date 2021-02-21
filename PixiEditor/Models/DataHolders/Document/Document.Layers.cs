using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.Enums;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Undo;

namespace PixiEditor.Models.DataHolders
{
    public partial class Document
    {
        private int activeLayerIndex;

        public ObservableCollection<Layer> Layers { get; set; } = new ObservableCollection<Layer>();

        public Layer ActiveLayer => Layers.Count > 0 ? Layers[ActiveLayerIndex] : null;

        public int ActiveLayerIndex
        {
            get => activeLayerIndex;
            set
            {
                activeLayerIndex = value;
                RaisePropertyChanged(nameof(ActiveLayerIndex));
                RaisePropertyChanged(nameof(ActiveLayer));
            }
        }

        public event EventHandler<LayersChangedEventArgs> LayersChanged;

        public void SetActiveLayer(int index)
        {
            if (ActiveLayerIndex <= Layers.Count - 1)
            {
                ActiveLayer.IsActive = false;
            }

            if (Layers.Any(x => x.IsActive))
            {
                var guids = Layers.Where(x => x.IsActive).Select(y => y.LayerGuid);
                guids.ToList().ForEach(x => Layers.First(layer => layer.LayerGuid == x).IsActive = false);
            }

            ActiveLayerIndex = index;
            ActiveLayer.IsActive = true;
            LayersChanged?.Invoke(this, new LayersChangedEventArgs(index, LayerAction.SetActive));
        }

        public void MoveLayerIndexBy(int layerIndex, int amount)
        {
            MoveLayerProcess(new object[] { layerIndex, amount });

            UndoManager.AddUndoChange(new Change(
                MoveLayerProcess,
                new object[] { layerIndex + amount, -amount },
                MoveLayerProcess,
                new object[] { layerIndex, amount },
                "Move layer"));
        }

        public void AddNewLayer(string name, WriteableBitmap bitmap, bool setAsActive = true)
        {
            AddNewLayer(name, bitmap.PixelWidth, bitmap.PixelHeight, setAsActive);
            Layers.Last().LayerBitmap = bitmap;
        }

        public void AddNewLayer(string name, bool setAsActive = true)
        {
            AddNewLayer(name, 0, 0, setAsActive);
        }

        public void AddNewLayer(string name, int width, int height, bool setAsActive = true)
        {
            Layers.Add(new Layer(name, width, height)
            {
                MaxHeight = Height,
                MaxWidth = Width
            });
            if (setAsActive)
            {
                SetActiveLayer(Layers.Count - 1);
            }

            if (Layers.Count > 1)
            {
                StorageBasedChange storageChange = new StorageBasedChange(this, new[] { Layers[^1] }, false);
                UndoManager.AddUndoChange(
                    storageChange.ToChange(
                        RemoveLayerProcess,
                        new object[] { Layers[^1].LayerGuid },
                        RestoreLayersProcess,
                        "Add layer"));
            }

            LayersChanged?.Invoke(this, new LayersChangedEventArgs(0, LayerAction.Add));
        }

        public void SetNextLayerAsActive(int lastLayerIndex)
        {
            if (Layers.Count > 0)
            {
                if (lastLayerIndex == 0)
                {
                    SetActiveLayer(0);
                }
                else
                {
                    SetActiveLayer(lastLayerIndex - 1);
                }
            }
        }

        public void RemoveLayer(int layerIndex)
        {
            if (Layers.Count == 0)
            {
                return;
            }

            bool wasActive = Layers[layerIndex].IsActive;

            StorageBasedChange change = new StorageBasedChange(this, new[] { Layers[layerIndex] });
            UndoManager.AddUndoChange(
                change.ToChange(RestoreLayersProcess, RemoveLayerProcess, new object[] { Layers[layerIndex].LayerGuid }, "Remove layer"));

            Layers.RemoveAt(layerIndex);
            if (wasActive)
            {
                SetNextLayerAsActive(layerIndex);
            }
        }

        public Layer MergeLayers(Layer[] layersToMerge, bool nameOfLast, int index)
        {
            if (layersToMerge == null || layersToMerge.Length < 2)
            {
                throw new ArgumentException("Not enough layers were provided to merge. Minimum amount is 2");
            }

            string name;

            // Wich name should be used
            if (nameOfLast)
            {
                name = layersToMerge[^1].Name;
            }
            else
            {
                name = layersToMerge[0].Name;
            }

            Layer mergedLayer = null;

            for (int i = 0; i < layersToMerge.Length - 1; i++)
            {
                Layer firstLayer = layersToMerge[i];
                Layer secondLayer = layersToMerge[i + 1];
                mergedLayer = firstLayer.MergeWith(secondLayer, name, Width, Height);

                // Insert new layer and remove old
                Layers.Insert(index, mergedLayer);
                Layers.Remove(firstLayer);
                Layers.Remove(secondLayer);

                SetActiveLayer(Layers.IndexOf(mergedLayer));
            }

            return mergedLayer;
        }

        public Layer MergeLayers(Layer[] layersToMerge, bool nameIsLastLayers)
        {
            if (layersToMerge == null || layersToMerge.Length < 2)
            {
                throw new ArgumentException("Not enough layers were provided to merge. Minimum amount is 2");
            }

            IEnumerable<Layer> undoArgs = layersToMerge;

            StorageBasedChange undoChange = new StorageBasedChange(this, undoArgs);

            int[] indexes = layersToMerge.Select(x => Layers.IndexOf(x)).ToArray();

            var layer = MergeLayers(layersToMerge, nameIsLastLayers, Layers.IndexOf(layersToMerge[0]));

            UndoManager.AddUndoChange(undoChange.ToChange(
                InsertLayersAtIndexesProcess,
                new object[] { indexes[0] },
                MergeLayersProcess,
                new object[] { indexes, nameIsLastLayers, layer.LayerGuid },
                "Undo merge layers"));

            return layer;
        }

        private void MergeLayersProcess(object[] args)
        {
            if (args.Length > 0
                && args[0] is int[] indexes
                && args[1] is bool nameOfSecond
                && args[2] is Guid mergedLayerGuid)
            {
                Layer[] layers = new Layer[indexes.Length];

                for (int i = 0; i < layers.Length; i++)
                {
                    layers[i] = Layers[indexes[i]];
                }

                Layer layer = MergeLayers(layers, nameOfSecond, indexes[0]);
                layer.ChangeGuid(mergedLayerGuid);
            }
        }

        private void InsertLayersAtIndexesProcess(Layer[] layers, UndoLayer[] data, object[] args)
        {
            if (args.Length > 0 && args[0] is int layerIndex)
            {
                Layers.RemoveAt(layerIndex);
                for (int i = 0; i < layers.Length; i++)
                {
                    Layer layer = layers[i];
                    Layers.Insert(data[i].LayerIndex, layer);
                }
            }
        }

        /// <summary>
        ///     Moves offsets of layers by specified vector.
        /// </summary>
        private void MoveOffsets(Coordinates moveVector)
        {
            for (int i = 0; i < Layers.Count; i++)
            {
                Thickness offset = Layers[i].Offset;
                Layers[i].Offset = new Thickness(offset.Left + moveVector.X, offset.Top + moveVector.Y, 0, 0);
            }
        }

        private void MoveOffsetsProcess(object[] arguments)
        {
            Coordinates vector = (Coordinates)arguments[0];
            MoveOffsets(vector);
        }

        private void MoveLayerProcess(object[] parameter)
        {
            int layerIndex = (int)parameter[0];
            int amount = (int)parameter[1];

            Layers.Move(layerIndex, layerIndex + amount);
            if (ActiveLayerIndex == layerIndex)
            {
                SetActiveLayer(layerIndex + amount);
            }
        }

        private void RestoreLayersProcess(Layer[] layers, UndoLayer[] layersData)
        {
            for (int i = 0; i < layers.Length; i++)
            {
                Layer layer = layers[i];

                Layers.Insert(layersData[i].LayerIndex, layer);
                if (layersData[i].IsActive)
                {
                    SetActiveLayer(Layers.IndexOf(layer));
                }
            }
        }

        private void RemoveLayerProcess(object[] parameters)
        {
            if (parameters != null && parameters.Length > 0 && parameters[0] is Guid layerGuid)
            {
                Layer layer = Layers.First(x => x.LayerGuid == layerGuid);
                int index = Layers.IndexOf(layer);
                bool wasActive = layer.IsActive;
                Layers.Remove(layer);

                if (wasActive || ActiveLayerIndex >= index)
                {
                    SetNextLayerAsActive(index);
                }
            }
        }
    }
}