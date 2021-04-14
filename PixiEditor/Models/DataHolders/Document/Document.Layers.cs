using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight.Messaging;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.Enums;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Undo;

namespace PixiEditor.Models.DataHolders
{
    public partial class Document
    {
        public const string MainSelectedLayerColor = "#505056";
        public const string SecondarySelectedLayerColor = "#7D505056";
        private Guid activeLayerGuid;
        private LayerStructure layerStructure;

        private ObservableCollection<Layer> layers = new ();

        public ObservableCollection<Layer> Layers
        {
            get => layers;
            set
            {
                layers = value;
                Layers.CollectionChanged += Layers_CollectionChanged;
            }
        }

        public LayerStructure LayerStructure
        {
            get => layerStructure;
            set
            {
                SetProperty(ref layerStructure, value);
            }
        }

        public Layer ActiveLayer => Layers.Count > 0 ? Layers.FirstOrDefault(x => x.LayerGuid == ActiveLayerGuid) : null;

        public Guid ActiveLayerGuid
        {
            get => activeLayerGuid;
            set
            {
                activeLayerGuid = value;
                RaisePropertyChanged(nameof(ActiveLayerGuid));
                RaisePropertyChanged(nameof(ActiveLayer));
            }
        }

        public event EventHandler<LayersChangedEventArgs> LayersChanged;

        public void SetMainActiveLayer(int index)
        {
            if (ActiveLayer != null && Layers.IndexOf(ActiveLayer) <= Layers.Count - 1)
            {
                ActiveLayer.IsActive = false;
            }

            foreach (var layer in Layers)
            {
                if (layer.IsActive)
                {
                    layer.IsActive = false;
                }
            }

            ActiveLayerGuid = Layers[index].LayerGuid;
            ActiveLayer.IsActive = true;
            LayersChanged?.Invoke(this, new LayersChangedEventArgs(ActiveLayerGuid, LayerAction.SetActive));
        }

        public void UpdateLayersColor()
        {
            foreach (var layer in Layers)
            {
                if (layer.LayerGuid == ActiveLayerGuid)
                {
                    layer.LayerHighlightColor = MainSelectedLayerColor;
                }
                else
                {
                    layer.LayerHighlightColor = SecondarySelectedLayerColor;
                }
            }
        }

        public void MoveLayerInStructure(Guid layerGuid, Guid referenceLayer, bool above = false)
        {
            var args = new object[] { layerGuid, referenceLayer, above };

            Layer layer = Layers.First(x => x.LayerGuid == layerGuid);

            int oldIndex = Layers.IndexOf(layer);

            MoveLayerInStructureProcess(args);

            UndoManager.AddUndoChange(new Change(
                ReverseMoveLayerInStructureProcess,
                new object[] { oldIndex, layerGuid },
                MoveLayerInStructureProcess,
                args,
                "Move layer"));
        }

        public void MoveFolderInStructure(Guid groupGuid, Guid referenceLayer, bool above = false)
        {
            var args = new object[] { groupGuid, referenceLayer, above };

            MoveFolderInStructureProcess(args);

            //UndoManager.AddUndoChange(new Change(
            //    ReverseMoveLayerInStructureProcess,
            //    new object[] { oldIndex, layerGuid, oldLayerFolder },
            //    MoveLayerInStructureProcess,
            //    args,
            //    "Move layer"));
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
                SetMainActiveLayer(Layers.Count - 1);
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

            LayersChanged?.Invoke(this, new LayersChangedEventArgs(Layers[^1].LayerGuid, LayerAction.Add));
        }

        public void SetNextLayerAsActive(int lastLayerIndex)
        {
            if (Layers.Count > 0)
            {
                if (lastLayerIndex == 0)
                {
                    SetMainActiveLayer(0);
                }
                else
                {
                    SetMainActiveLayer(lastLayerIndex - 1);
                }
            }
        }

        public void SetNextSelectedLayerAsActive(Guid lastLayerGuid)
        {
            var selectedLayers = Layers.Where(x => x.IsActive);
            foreach (var layer in selectedLayers)
            {
                if (layer.LayerGuid != lastLayerGuid)
                {
                    ActiveLayerGuid = layer.LayerGuid;
                    LayersChanged?.Invoke(this, new LayersChangedEventArgs(ActiveLayerGuid, LayerAction.SetActive));
                    return;
                }
            }
        }

        public void ToggleLayer(int index)
        {
            if (index < Layers.Count && index >= 0)
            {
                Layer layer = Layers[index];
                if (layer.IsActive && Layers.Count(x => x.IsActive) == 1)
                {
                    return;
                }

                if (ActiveLayerGuid == layer.LayerGuid)
                {
                    SetNextSelectedLayerAsActive(layer.LayerGuid);
                }

                layer.IsActive = !layer.IsActive;
            }
        }

        /// <summary>
        /// Selects all layers between active layer and layer at given index.
        /// </summary>
        /// <param name="index">End of range index.</param>
        public void SelectLayersRange(int index)
        {
            DeselectAllExcept(ActiveLayer);
            int firstIndex = Layers.IndexOf(ActiveLayer);

            int startIndex = Math.Min(index, firstIndex);
            for (int i = startIndex; i <= startIndex + Math.Abs(index - firstIndex); i++)
            {
                Layers[i].IsActive = true;
            }
        }

        public void DeselectAllExcept(Layer exceptLayer)
        {
            foreach (var layer in Layers)
            {
                if (layer == exceptLayer)
                {
                    continue;
                }

                layer.IsActive = false;
            }
        }

        public void RemoveLayer(int layerIndex, bool addToUndo)
        {
            if (Layers.Count == 0)
            {
                return;
            }

            LayerStructure.AssignParent(Layers[layerIndex].LayerGuid, null);

            bool wasActive = Layers[layerIndex].IsActive;

            StorageBasedChange change = new StorageBasedChange(this, new[] { Layers[layerIndex] });
            if (addToUndo)
            {
                UndoManager.AddUndoChange(
                    change.ToChange(RestoreLayersProcess, RemoveLayerProcess, new object[] { Layers[layerIndex].LayerGuid }, "Remove layer"));

            }

            Layers.RemoveAt(layerIndex);
            if (wasActive)
            {
                SetNextLayerAsActive(layerIndex);
            }
        }

        public void RemoveLayer(Layer layer, bool addToUndo)
        {
            RemoveLayer(Layers.IndexOf(layer), addToUndo);
        }

        public void RemoveActiveLayers()
        {
            if (Layers.Count == 0 || !Layers.Any(x => x.IsActive))
            {
                return;
            }

            Layer[] layers = Layers.Where(x => x.IsActive).ToArray();
            int firstIndex = Layers.IndexOf(layers[0]);

            object[] guidArgs = new object[] { layers.Select(x => x.LayerGuid).ToArray() };

            StorageBasedChange change = new StorageBasedChange(this, layers);

            RemoveLayersProcess(guidArgs);

            InjectRemoveActiveLayersUndo(guidArgs, change);

            SetNextLayerAsActive(firstIndex);
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

            Layer mergedLayer = layersToMerge[0];

            var groupParent = LayerStructure.GetGroupByLayer(layersToMerge[^1].LayerGuid);

            Layer placeholderLayer = new("_placeholder");
            Layers.Insert(index, placeholderLayer);
            LayerStructure.AssignParent(placeholderLayer.LayerGuid, groupParent);

            for (int i = 0; i < layersToMerge.Length - 1; i++)
            {
                Layer firstLayer = mergedLayer;
                Layer secondLayer = layersToMerge[i + 1];
                mergedLayer = firstLayer.MergeWith(secondLayer, name, Width, Height);
                RemoveLayer(layersToMerge[i], false);
            }

            Layers.Insert(index, mergedLayer);
            LayerStructure.AssignParent(mergedLayer.LayerGuid, groupParent);

            RemoveLayer(placeholderLayer, false);

            RemoveLayer(layersToMerge[^1], false);

            SetMainActiveLayer(Layers.IndexOf(mergedLayer));

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

        private void ReverseMoveLayerInStructureProcess(object[] props)
        {
            int indexTo = (int)props[0];
            Guid layerGuid = (Guid)props[1];

            Guid layerAtOldIndex = Layers[indexTo].LayerGuid;

            var startFolder = LayerStructure.GetGroupByLayer(layerGuid);

            LayerStructure.PreMoveReassignBounds(startFolder, layerGuid);

            Layers.Move(Layers.IndexOf(Layers.First(x => x.LayerGuid == layerGuid)), indexTo);

            var newFolder = LayerStructure.GetGroupByLayer(layerAtOldIndex);

            LayerStructure.PostMoveReassignBounds(newFolder, layerGuid);

            RaisePropertyChanged(nameof(LayerStructure));
        }

        private void InjectRemoveActiveLayersUndo(object[] guidArgs, StorageBasedChange change)
        {
            Action<Layer[], UndoLayer[]> undoAction = RestoreLayersProcess;
            Action<object[]> redoAction = RemoveLayersProcess;

            if (Layers.Count == 0)
            {
                Layer layer = new Layer("Base Layer");
                Layers.Add(layer);
                undoAction = (Layer[] layers, UndoLayer[] undoData) =>
                {
                    Layers.RemoveAt(0);
                    RestoreLayersProcess(layers, undoData);
                };
                redoAction = (object[] args) =>
                {
                    RemoveLayersProcess(args);
                    Layers.Add(layer);
                };
            }

            UndoManager.AddUndoChange(
            change.ToChange(
                undoAction,
                redoAction,
                guidArgs,
                "Remove layers"));
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
                    layer.IsActive = true;
                    Layers.Insert(data[i].LayerIndex, layer);
                }

                ActiveLayerGuid = layers.First(x => x.LayerHighlightColor == MainSelectedLayerColor).LayerGuid;
                // Identifying main layer by highlightColor is a bit hacky, but shhh
            }
        }

        /// <summary>
        ///     Moves offsets of layers by specified vector.
        /// </summary>
        private void MoveOffsets(IEnumerable<Layer> layers, Coordinates moveVector)
        {
            foreach (Layer layer in layers)
            {
                Thickness offset = layer.Offset;
                layer.Offset = new Thickness(offset.Left + moveVector.X, offset.Top + moveVector.Y, 0, 0);
            }
        }

        private void MoveOffsetsProcess(object[] arguments)
        {
            if (arguments.Length > 0 && arguments[0] is IEnumerable<Layer> layers && arguments[1] is Coordinates vector)
            {
                MoveOffsets(layers, vector);
            }
            else
            {
                throw new ArgumentException("Provided arguments were invalid. Expected IEnumerable<Layer> and Coordinates");
            }
        }

        private void MoveFolderInStructureProcess(object[] parameter)
        {
            Guid folderGuid = (Guid)parameter[0];
            Guid referenceLayerGuid = (Guid)parameter[1];
            bool above = (bool)parameter[2];

            GuidStructureItem group = LayerStructure.GetGroupByGuid(folderGuid);
            GuidStructureItem referenceLayergroup = LayerStructure.GetGroupByLayer(referenceLayerGuid);

            Layer referenceLayer = Layers.First(x => x.LayerGuid == referenceLayerGuid);

            int layerIndex = Layers.IndexOf(referenceLayer);
            int folderTopIndex = Layers.IndexOf(Layers.First(x => x.LayerGuid == group.EndLayerGuid));
            int oldIndex = folderTopIndex;

            if (layerIndex < folderTopIndex)
            {
                int folderBottomIndex = Layers.IndexOf(Layers.First(x => x.LayerGuid == group.StartLayerGuid));
                oldIndex = folderBottomIndex;
            }

            int newIndex = CalculateNewIndex(layerIndex, above, oldIndex);

            LayerStructure.MoveGroup(folderGuid, group.Parent, newIndex);

            ReassignParent(group, referenceLayergroup);

            LayerStructure.PostMoveReassignBounds(group.Parent, group);
        }

        private void ReassignParent(GuidStructureItem folder, GuidStructureItem referenceLayerFolder)
        {
            folder.Parent?.Subgroups.Remove(folder);
            if (LayerStructure.Groups.Contains(folder))
            {
                LayerStructure.Groups.Remove(folder);
            }

            if (referenceLayerFolder == null)
            {
                if (!LayerStructure.Groups.Contains(folder))
                {
                    LayerStructure.Groups.Add(folder);
                    folder.Parent = null;
                }
            }
            else
            {
                referenceLayerFolder.Subgroups.Add(folder);
                folder.Parent = referenceLayerFolder;
            }
        }

        private int CalculateNewIndex(int layerIndex, bool above, int oldIndex)
        {
            int newIndex = layerIndex;

            if ((oldIndex - layerIndex == -1 && !above) || (oldIndex - layerIndex == 1 && above))
            {
                newIndex += above ? 1 : -1;
            }
            else if (above)
            {
                newIndex++;
            }

            return Math.Clamp(newIndex, 0, Layers.Count - 1);
        }

        private void MoveLayerInStructureProcess(object[] parameter)
        {
            Guid layer = (Guid)parameter[0];
            Guid referenceLayer = (Guid)parameter[1];
            bool above = (bool)parameter[2];

            int layerIndex = Layers.IndexOf(Layers.First(x => x.LayerGuid == referenceLayer));
            int oldIndex = Layers.IndexOf(Layers.First(x => x.LayerGuid == layer));
            int newIndex = CalculateNewIndex(layerIndex, above, oldIndex);

            var startFolder = LayerStructure.GetGroupByLayer(layer);

            LayerStructure.PreMoveReassignBounds(startFolder, layer);

            Layers.Move(oldIndex, newIndex);

            var newFolder = LayerStructure.GetGroupByLayer(referenceLayer);

            LayerStructure.PostMoveReassignBounds(newFolder, layer);

            if (Layers.IndexOf(ActiveLayer) == oldIndex)
            {
                SetMainActiveLayer(newIndex);
            }

            RaisePropertyChanged(nameof(LayerStructure));
        }

        private void RestoreLayersProcess(Layer[] layers, UndoLayer[] layersData)
        {
            for (int i = 0; i < layers.Length; i++)
            {
                Layer layer = layers[i];

                Layers.Insert(layersData[i].LayerIndex, layer);
                if (layersData[i].IsActive)
                {
                    SetMainActiveLayer(Layers.IndexOf(layer));
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

                var layerGroup = LayerStructure.GetGroupByLayer(layer.LayerGuid);

                LayerStructure.AssignParent(Layers[index].LayerGuid, null);
                RemoveGroupsIfEmpty(layer, layerGroup);

                Layers.Remove(layer);

                if (wasActive || Layers.IndexOf(ActiveLayer) >= index)
                {
                    SetNextLayerAsActive(index);
                }
            }
        }

        private void RemoveGroupsIfEmpty(Layer layer, GuidStructureItem layerGroup)
        {
            if (LayerStructure.GroupContainsOnlyLayer(layer, layerGroup))
            {
                if (layerGroup.Parent != null)
                {
                    layerGroup.Parent.Subgroups.Remove(layerGroup);
                    RemoveGroupsIfEmpty(layer, layerGroup.Parent);
                }
                else
                {
                    LayerStructure.Groups.Remove(layerGroup);
                }
            }
        }

        private void RemoveLayersProcess(object[] parameters)
        {
            if (parameters != null && parameters.Length > 0 && parameters[0] is IEnumerable<Guid> layerGuids)
            {
                object[] args = new object[1];
                foreach (var guid in layerGuids)
                {
                    args[0] = guid;
                    RemoveLayerProcess(args);
                }
            }
        }
    }
}