using PixiEditor.Helpers;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.Enums;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Layers.Utils;
using PixiEditor.Models.Position;
using PixiEditor.Models.Undo;
using SkiaSharp;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

namespace PixiEditor.Models.DataHolders
{
    public partial class Document
    {
        public const string MainSelectedLayerColor = "#505056";
        public const string SecondarySelectedLayerColor = "#7D505056";
        private static readonly Regex reversedLayerSuffixRegex = new(@"(?:\)([0-9]+)*\()? *([\s\S]+)", RegexOptions.Compiled);
        private Guid activeLayerGuid;
        private LayerStructure layerStructure;

        private WpfObservableRangeCollection<Layer> layers = new();

        public WpfObservableRangeCollection<Layer> Layers
        {
            get => layers;
            set
            {
                layers = value;
                Layers.CollectionChanged += Layers_CollectionChanged;
                Renderer.SetNewLayersCollection(value);
            }
        }

        public LayerStructure LayerStructure
        {
            get => layerStructure;
            private set
            {
                layerStructure = value;
                RaisePropertyChanged(nameof(LayerStructure));
            }
        }

        private LayerStackRenderer renderer;
        public LayerStackRenderer Renderer
        {
            get => renderer;
            private set
            {
                renderer = value;
                RaisePropertyChanged(nameof(Renderer));
            }
        }

        private Layer referenceLayer;
        private SingleLayerRenderer referenceLayerRenderer;
        public Layer ReferenceLayer
        {
            get => referenceLayer;
            set
            {
                referenceLayer = value;
                referenceLayerRenderer?.Dispose();
                referenceLayerRenderer = referenceLayer == null ? null : new SingleLayerRenderer(referenceLayer, referenceLayer.Width, referenceLayer.Height);
                RaisePropertyChanged(nameof(ReferenceLayer));
                RaisePropertyChanged(nameof(ReferenceLayerRenderer));
            }
        }

        public SingleLayerRenderer ReferenceLayerRenderer
        {
            get => referenceLayerRenderer;
        }

        public Layer ActiveLayer => Layers.Count > 0 ? Layers.FirstOrDefault(x => x.GuidValue == ActiveLayerGuid) : null;

        public Guid ActiveLayerGuid
        {
            get => activeLayerGuid;
            set
            {
                if (value != activeLayerGuid)
                {
                    activeLayerGuid = value;
                    RaisePropertyChanged(nameof(ActiveLayerGuid));
                    RaisePropertyChanged(nameof(ActiveLayer));
                }
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

            ActiveLayerGuid = Layers[index].GuidValue;
            ActiveLayer.IsActive = true;
            LayersChanged?.Invoke(this, new LayersChangedEventArgs(ActiveLayerGuid, LayerAction.SetActive));
        }

        /// <summary>
        /// Gets final layer IsVisible taking into consideration group visibility.
        /// </summary>
        /// <param name="layer">Layer to check.</param>
        /// <returns>True if is visible, false if at least parent is not visible or layer itself is invisible.</returns>
        public bool GetFinalLayerIsVisible(Layer layer) => LayerStructureUtils.GetFinalLayerIsVisible(layer, LayerStructure);
        public void UpdateLayersColor()
        {
            foreach (var layer in Layers)
            {
                if (layer.GuidValue == ActiveLayerGuid)
                {
                    layer.LayerHighlightColor = MainSelectedLayerColor;
                }
                else
                {
                    layer.LayerHighlightColor = SecondarySelectedLayerColor;
                }
            }
        }

        public void MoveLayerInStructure(Guid layerGuid, Guid referenceLayer, bool above = false, bool addToUndo = true)
        {
            var args = new object[] { layerGuid, referenceLayer, above };

            Layer layer = Layers.First(x => x.GuidValue == layerGuid);

            int oldIndex = Layers.IndexOf(layer);

            var oldLayerStrcutureGroups = LayerStructure.CloneGroups();

            MoveLayerInStructureProcess(args);

            AddLayerStructureToUndo(oldLayerStrcutureGroups);

            if (!addToUndo) return;

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

            var topLayer = Layers.First(x => x.GuidValue == LayerStructure.GetGroupByGuid(groupGuid).EndLayerGuid);
            var bottomLayer = Layers.First(x => x.GuidValue == LayerStructure.GetGroupByGuid(groupGuid).StartLayerGuid);

            int indexOfTopLayer = Layers.IndexOf(topLayer);
            Guid oldReferenceLayerGuid;
            bool oldAbove = false;

            if (indexOfTopLayer + 1 < Layers.Count)
            {
                oldReferenceLayerGuid = topLayer.GuidValue;
            }
            else
            {
                int indexOfBottomLayer = Layers.IndexOf(bottomLayer);
                oldReferenceLayerGuid = Layers[indexOfBottomLayer - 1].GuidValue;
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

        public void AddNewLayer(string name, Surface bitmap, bool setAsActive = true)
        {
            AddNewLayer(name, bitmap.Width, bitmap.Height, setAsActive, bitmap);
        }

        public void AddNewLayer(string name, bool setAsActive = true)
        {
            AddNewLayer(name, 1, 1, setAsActive);
        }

        public void AddNewLayer(string name, int width, int height, bool setAsActive = true, Surface bitmap = null)
        {
            Layer layer;

            if (bitmap != null)
            {
                if (width != bitmap.Width || height != bitmap.Height)
                    throw new ArgumentException("Inconsistent width and height");
            }
            if (width <= 0 || height <= 0)
                throw new ArgumentException("Dimensions must be greater than 0");

            layer = bitmap == null ? new Layer(name, width, height) : new Layer(name, bitmap);
            layer.MaxHeight = Height;
            layer.MaxWidth = Width;

            Layers.Add(layer);

            layer.Name = GetLayerSuffix(layer);

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
                        new object[] { Layers[^1].GuidValue },
                        RestoreLayersProcess,
                        "Add layer"));
            }

            LayersChanged?.Invoke(this, new LayersChangedEventArgs(Layers[^1].GuidValue, LayerAction.Add));
        }

        /// <summary>
        /// Duplicates the layer at the <paramref name="index"/>.
        /// </summary>
        /// <param name="index">The index of the layer to duplicate.</param>
        /// <returns>The duplicate.</returns>
        public Layer DuplicateLayer(int index)
        {
            Layer duplicate = Layers[index].Clone(true);

            duplicate.Name = GetLayerSuffix(duplicate);

            Layers.Insert(index + 1, duplicate);
            SetMainActiveLayer(index + 1);

            StorageBasedChange storageChange = new(this, new[] { duplicate }, false);
            UndoManager.AddUndoChange(
                storageChange.ToChange(
                    RemoveLayerProcess,
                    new object[] { duplicate.GuidValue },
                    RestoreLayersProcess,
                    "Duplicate Layer"));

            return duplicate;
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
                if (layer.GuidValue != lastLayerGuid)
                {
                    ActiveLayerGuid = layer.GuidValue;
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

                if (ActiveLayerGuid == layer.GuidValue)
                {
                    SetNextSelectedLayerAsActive(layer.GuidValue);
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

            LayerStructure.AssignParent(Layers[layerIndex].GuidValue, null);

            bool wasActive = Layers[layerIndex].IsActive;

            if (addToUndo)
            {
                StorageBasedChange change = new(this, new[] { Layers[layerIndex] });
                UndoManager.AddUndoChange(
                    change.ToChange(RestoreLayersProcess, RemoveLayerProcess, new object[] { Layers[layerIndex].GuidValue }));
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

            object[] guidArgs = new object[] { layers.Select(x => x.GuidValue).ToArray() };

            StorageBasedChange change = new(this, layers);

            RemoveLayersProcess(guidArgs);

            AddLayerStructureToUndo(oldLayerStructure);

            InjectRemoveActiveLayersUndo(guidArgs, change);

            UndoManager.SquashUndoChanges(2, "Removed active layers");

            SetNextLayerAsActive(firstIndex);

        }

        public void AddLayerStructureToUndo(WpfObservableRangeCollection<GuidStructureItem> oldLayerStructureGroups)
        {
            UndoManager.AddUndoChange(
                new Change(
                    BuildLayerStructureProcess,
                    new object[] { oldLayerStructureGroups },
                    BuildLayerStructureProcess,
                    new object[] { LayerStructure.CloneGroups() }, "Reload LayerStructure"));
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

            var groupParent = LayerStructure.GetGroupByLayer(layersToMerge[^1].GuidValue);

            Layer placeholderLayer = new("_placeholder");
            Layers.Insert(index, placeholderLayer);
            LayerStructure.AssignParent(placeholderLayer.GuidValue, groupParent?.GroupGuid);

            for (int i = 0; i < layersToMerge.Length - 1; i++)
            {
                Layer firstLayer = mergedLayer;
                Layer secondLayer = layersToMerge[i + 1];
                mergedLayer = firstLayer.MergeWith(secondLayer, name, Width, Height);
                RemoveLayer(layersToMerge[i], false);
            }

            Layers.Insert(index, mergedLayer);
            LayerStructure.AssignParent(mergedLayer.GuidValue, groupParent?.GroupGuid);

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
                new object[] { indexes, nameIsLastLayers, layer.GuidValue }));

            UndoManager.SquashUndoChanges(2, "Undo merge layers");

            return layer;
        }

        public SKColor GetColorAtPoint(int x, int y)
        {
            return Renderer.FinalSurface.GetSRGBPixel(x, y);
        }

        private void DisposeLayerBitmaps()
        {
            foreach (var layer in layers)
            {
                layer.LayerBitmap.Dispose();
            }

            referenceLayer?.LayerBitmap.Dispose();
            previewLayer?.LayerBitmap.Dispose();

            previewLayerRenderer?.Dispose();
            referenceLayerRenderer?.Dispose();
            renderer?.Dispose();
        }

        public void BuildLayerStructureProcess(object[] parameters)
        {
            if (parameters.Length > 0 && parameters[0] is WpfObservableRangeCollection<GuidStructureItem> groups)
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

            Guid layerAtOldIndex = Layers[indexTo].GuidValue;

            var startGroup = LayerStructure.GetGroupByLayer(layerGuid);

            LayerStructure.PreMoveReassignBounds(new GroupData(startGroup?.GroupGuid), layerGuid);

            Layers.Move(Layers.IndexOf(Layers.First(x => x.GuidValue == layerGuid)), indexTo);

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
                Layer layer = new("Base Layer", 1, 1) { MaxHeight = Height, MaxWidth = Width };
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
                RemoveLayer(layerIndex, false);

                for (int i = 0; i < layers.Length; i++)
                {
                    Layer layer = layers[i];
                    layer.IsActive = true;
                    Layers.Insert(data[i].LayerIndex, layer);
                }

                ActiveLayerGuid = layers.First(x => x.LayerHighlightColor == MainSelectedLayerColor).GuidValue;
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

            Layer referenceLayer = Layers.First(x => x.GuidValue == referenceLayerGuid);

            int layerIndex = Layers.IndexOf(referenceLayer);
            int folderTopIndex = Layers.IndexOf(Layers.First(x => x.GuidValue == group?.EndLayerGuid));
            int oldIndex = folderTopIndex;

            if (layerIndex < folderTopIndex)
            {
                int folderBottomIndex = Layers.IndexOf(Layers.First(x => x.GuidValue == group.StartLayerGuid));
                oldIndex = folderBottomIndex;
            }

            int newIndex = CalculateNewIndex(layerIndex, above, oldIndex);

            LayerStructure.MoveGroup(groupGuid, newIndex);

            LayerStructure.ReassignParent(group, referenceLayerGroup);

            LayerStructure.PostMoveReassignBounds(new GroupData(group?.Parent?.GroupGuid), new GroupData(group?.GroupGuid));
        }

        private int CalculateNewIndex(int layerIndex, bool above, int oldIndex)
        {
            int newIndex = layerIndex;

            int diff = newIndex - oldIndex;

            if (TriesToMoveAboveBelow(above, diff) || TriesToMoveBelowAbove(above, diff) || (above && newIndex < oldIndex) || (!above && newIndex > oldIndex))
            {
                newIndex += above ? 1 : -1;
            }

            return Math.Clamp(newIndex, 0, Layers.Count - 1);
        }

        private bool TriesToMoveAboveBelow(bool above, int diff) => above && diff == -1;

        private bool TriesToMoveBelowAbove(bool above, int diff) => !above && diff == 1;

        private void MoveLayerInStructureProcess(object[] parameter)
        {
            Guid layer = (Guid)parameter[0];
            Guid referenceLayer = (Guid)parameter[1];
            bool above = (bool)parameter[2];

            int layerIndex = Layers.IndexOf(Layers.First(x => x.GuidValue == referenceLayer));
            int oldIndex = Layers.IndexOf(Layers.First(x => x.GuidValue == layer));
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
            Renderer.ForceRerender();
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
            if (parameters is { Length: > 0 } && parameters[0] is Guid layerGuid)
            {
                Layer layer = Layers.First(x => x.GuidValue == layerGuid);
                int index = Layers.IndexOf(layer);
                bool wasActive = layer.IsActive;

                var layerGroup = LayerStructure.GetGroupByLayer(layer.GuidValue);

                LayerStructure.ExpandParentGroups(layerGroup);

                if (layerGroup?.Parent != null && LayerStructure.GroupContainsOnlyLayer(layer.GuidValue, layerGroup))
                {
                    LayerStructure.PreMoveReassignBounds(new GroupData(layerGroup.Parent.GroupGuid), new GroupData(layerGroup.GroupGuid));
                }
                LayerStructure.AssignParent(Layers[index].GuidValue, null);
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
            if (LayerStructure.GroupContainsOnlyLayer(layer.GuidValue, layerGroup))
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

        /// <summary>
        /// Get's the layers suffix, e.g. "New Layer" becomes "New Layer (1)".
        /// </summary>
        /// <returns>Name of the layer with suffix.</returns>
        private string GetLayerSuffix(Layer layer)
        {
            Match match = reversedLayerSuffixRegex.Match(layer.Name.Reverse());

            int? highestValue = GetHighestSuffix(layer, match.Groups[2].Value, reversedLayerSuffixRegex);

            string actualName = match.Groups[2].Value.Reverse();

            if (highestValue == null)
            {
                return actualName;
            }

            return actualName + $" ({highestValue + 1})";
        }

        private int? GetHighestSuffix(Layer except, string layerName, Regex regex)
        {
            int? highestValue = null;

            foreach (Layer otherLayer in Layers)
            {
                if (otherLayer == except)
                {
                    continue;
                }

                Match otherMatch = regex.Match(otherLayer.Name.Reverse());

                if (otherMatch.Groups[2].Value == layerName)
                {
                    SetHighest(otherMatch.Groups[1].Value.Reverse(), ref highestValue);
                }
            }

            return highestValue;
        }

        /// <returns>Was the parse a sucess.</returns>
        private bool SetHighest(string number, ref int? highest, int? defaultValue = 0)
        {
            bool sucess = int.TryParse(number, out int parsedNumber);

            if (sucess)
            {
                if (highest == null || highest < parsedNumber)
                {
                    highest = parsedNumber;
                }
            }
            else
            {
                if (highest == null)
                {
                    highest = defaultValue;
                }
            }

            return sucess;
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
