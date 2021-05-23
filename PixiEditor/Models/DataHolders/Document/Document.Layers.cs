using System;
using System.Buffers;
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
                layerStructure = value;
                RaisePropertyChanged(nameof(LayerStructure));
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
            ActiveLayer!.IsActive = true;
            LayersChanged?.Invoke(this, new LayersChangedEventArgs(ActiveLayerGuid, LayerAction.SetActive));
        }

        /// <summary>
        /// Gets final layer IsVisible taking into consideration group visibility.
        /// </summary>
        /// <param name="layer">Layer to check.</param>
        /// <returns>True if is visible, false if at least parent is not visible or layer itself is invisible.</returns>
        public bool GetFinalLayerIsVisible(Layer layer)
        {
            if (!layer.IsVisible)
            {
                return false;
            }

            var group = LayerStructure.GetGroupByLayer(layer.LayerGuid);
            bool atLeastOneParentIsInvisible = false;
            GuidStructureItem groupToCheck = group;
            while(groupToCheck != null)
            {
                if (!groupToCheck.IsVisible)
                {
                    atLeastOneParentIsInvisible = true;
                    break;
                }

                groupToCheck = groupToCheck.Parent;
            }

            return !atLeastOneParentIsInvisible;
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

            var oldLayerStrcutureGroups = LayerStructure.CloneGroups();

            MoveLayerInStructureProcess(args);

            AddLayerStructureToUndo(oldLayerStrcutureGroups);

            UndoManager.AddUndoChange(new Change(
                ReverseMoveLayerInStructureProcess,
                new object[] { oldIndex, layerGuid },
                MoveLayerInStructureProcess,
                args,
                "Move layer"));

            UndoManager.SquashUndoChanges(2, "Move layer");
        }

        public void MoveGroupInStructure(Guid groupGuid, Guid referenceLayer, bool above = false)
        {
            var args = new object[] { groupGuid, referenceLayer, above };

            var topLayer = Layers.First(x => x.LayerGuid == LayerStructure.GetGroupByGuid(groupGuid).EndLayerGuid);
            var bottomLayer = Layers.First(x => x.LayerGuid == LayerStructure.GetGroupByGuid(groupGuid).StartLayerGuid);

            int indexOfTopLayer = Layers.IndexOf(topLayer);
            Guid oldReferenceLayerGuid;
            bool oldAbove = false;

            if(indexOfTopLayer + 1 < Layers.Count)
            {
                oldReferenceLayerGuid = topLayer.LayerGuid;
            }
            else
            {
                int indexOfBottomLayer = Layers.IndexOf(bottomLayer);
                oldReferenceLayerGuid = Layers[indexOfBottomLayer - 1].LayerGuid;
                oldAbove = true;
            }

            var oldLayerStructure = LayerStructure.CloneGroups();

            MoveGroupInStructureProcess(args);

            AddLayerStructureToUndo(oldLayerStructure);

            UndoManager.AddUndoChange(new Change(
                MoveGroupInStructureProcess,
                new object[] { groupGuid, oldReferenceLayerGuid, oldAbove },
                MoveGroupInStructureProcess,
                args));

            UndoManager.SquashUndoChanges(2, "Move group");
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
                StorageBasedChange storageChange = new(this, new[] { Layers[^1] }, false);
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

        public void RemoveLayer(int layerIndex, bool addToUndo = true)
        {
            if (Layers.Count == 0)
            {
                return;
            }

            LayerStructure.AssignParent(Layers[layerIndex].LayerGuid, null);

            bool wasActive = Layers[layerIndex].IsActive;

            StorageBasedChange change = new(this, new[] { Layers[layerIndex] });
            if (addToUndo)
            {
                UndoManager.AddUndoChange(
                    change.ToChange(RestoreLayersProcess, RemoveLayerProcess, new object[] { Layers[layerIndex].LayerGuid }));
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

            var oldLayerStructure = LayerStructure.CloneGroups();

            Layer[] layers = Layers.Where(x => x.IsActive).ToArray();
            int firstIndex = Layers.IndexOf(layers[0]);

            object[] guidArgs = new object[] { layers.Select(x => x.LayerGuid).ToArray() };

            StorageBasedChange change = new(this, layers);

            RemoveLayersProcess(guidArgs);

            AddLayerStructureToUndo(oldLayerStructure);

            InjectRemoveActiveLayersUndo(guidArgs, change);

            UndoManager.SquashUndoChanges(2, "Removed active layers");

            SetNextLayerAsActive(firstIndex);

        }

        public void AddLayerStructureToUndo(ObservableCollection<GuidStructureItem> oldLayerStructureGroups)
        {
            UndoManager.AddUndoChange(
                new Change(
                    BuildLayerStructureProcess,
                    new[] { oldLayerStructureGroups },
                    BuildLayerStructureProcess,
                    new[] { LayerStructure.CloneGroups() }));
        }

        public Layer MergeLayers(Layer[] layersToMerge, bool nameOfLast, int index)
        {
            if (layersToMerge == null || layersToMerge.Length < 2)
            {
                throw new ArgumentException("Not enough layers were provided to merge. Minimum amount is 2");
            }

            string name;

            // Which name should be used
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
            LayerStructure.AssignParent(placeholderLayer.LayerGuid, groupParent?.GroupGuid);

            for (int i = 0; i < layersToMerge.Length - 1; i++)
            {
                Layer firstLayer = mergedLayer;
                Layer secondLayer = layersToMerge[i + 1];
                mergedLayer = firstLayer.MergeWith(secondLayer, name, Width, Height);
                RemoveLayer(layersToMerge[i], false);
            }

            Layers.Insert(index, mergedLayer);
            LayerStructure.AssignParent(mergedLayer.LayerGuid, groupParent?.GroupGuid);

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

            var oldLayerStructure = LayerStructure.CloneGroups();

            StorageBasedChange undoChange = new(this, undoArgs);

            int[] indexes = layersToMerge.Select(x => Layers.IndexOf(x)).ToArray();

            var layer = MergeLayers(layersToMerge, nameIsLastLayers, Layers.IndexOf(layersToMerge[0]));

            AddLayerStructureToUndo(oldLayerStructure);

            UndoManager.AddUndoChange(undoChange.ToChange(
                InsertLayersAtIndexesProcess,
                new object[] { indexes[0] },
                MergeLayersProcess,
                new object[] { indexes, nameIsLastLayers, layer.LayerGuid }));

            UndoManager.SquashUndoChanges(2, "Undo merge layers");

            return layer;
        }

        private void BuildLayerStructureProcess(object[] parameters)
        {
            if(parameters.Length > 0 && parameters[0] is ObservableCollection<GuidStructureItem> groups)
            {
                LayerStructure.Groups.CollectionChanged -= Groups_CollectionChanged;
                LayerStructure.Groups = LayerStructure.CloneGroups(groups);
                LayerStructure.Groups.CollectionChanged += Groups_CollectionChanged;
                RaisePropertyChanged(nameof(LayerStructure));
            }
        }

        private void ReverseMoveLayerInStructureProcess(object[] props)
        {
            int indexTo = (int)props[0];
            Guid layerGuid = (Guid)props[1];

            Guid layerAtOldIndex = Layers[indexTo].LayerGuid;

            var startGroup = LayerStructure.GetGroupByLayer(layerGuid);

            LayerStructure.PreMoveReassignBounds(new GroupData(startGroup?.GroupGuid), layerGuid);

            Layers.Move(Layers.IndexOf(Layers.First(x => x.LayerGuid == layerGuid)), indexTo);

            var newGroup = LayerStructure.GetGroupByLayer(layerAtOldIndex);

            LayerStructure.PostMoveReassignBounds(new GroupData(newGroup?.GroupGuid), layerGuid);

            RaisePropertyChanged(nameof(LayerStructure));
        }

        private void InjectRemoveActiveLayersUndo(object[] guidArgs, StorageBasedChange change)
        {
            Action<Layer[], UndoLayer[]> undoAction = RestoreLayersProcess;
            Action<object[]> redoAction = RemoveLayersProcess;

            if (Layers.Count == 0)
            {
                Layer layer = new("Base Layer");
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

        private void MoveGroupInStructureProcess(object[] parameter)
        {
            Guid groupGuid = (Guid)parameter[0];
            Guid referenceLayerGuid = (Guid)parameter[1];
            bool above = (bool)parameter[2];

            GuidStructureItem group = LayerStructure.GetGroupByGuid(groupGuid);
            GuidStructureItem referenceLayerGroup = LayerStructure.GetGroupByLayer(referenceLayerGuid);

            Layer referenceLayer = Layers.First(x => x.LayerGuid == referenceLayerGuid);

            int layerIndex = Layers.IndexOf(referenceLayer);
            int folderTopIndex = Layers.IndexOf(Layers.First(x => x.LayerGuid == group?.EndLayerGuid));
            int oldIndex = folderTopIndex;

            if (layerIndex < folderTopIndex)
            {
                int folderBottomIndex = Layers.IndexOf(Layers.First(x => x.LayerGuid == group.StartLayerGuid));
                oldIndex = folderBottomIndex;
            }

            int newIndex = CalculateNewIndex(layerIndex, above, oldIndex);

            LayerStructure.MoveGroup(groupGuid, newIndex);

            ReassignParent(group, referenceLayerGroup);

            LayerStructure.PostMoveReassignBounds(new GroupData(group?.Parent?.GroupGuid), new GroupData(group?.GroupGuid));
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

            var startGroup = LayerStructure.GetGroupByLayer(layer);

            LayerStructure.PreMoveReassignBounds(new GroupData(startGroup?.GroupGuid), layer);

            Layers.Move(oldIndex, newIndex);

            var newFolder = LayerStructure.GetGroupByLayer(referenceLayer);

            LayerStructure.PostMoveReassignBounds(new GroupData(newFolder?.GroupGuid), layer);

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

                LayersChanged?.Invoke(this, new LayersChangedEventArgs(layerGuid, LayerAction.Remove));
            }
        }

        private void RemoveGroupsIfEmpty(Layer layer, GuidStructureItem layerGroup)
        {
            if (LayerStructure.GroupContainsOnlyLayer(layer.LayerGuid, layerGroup))
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