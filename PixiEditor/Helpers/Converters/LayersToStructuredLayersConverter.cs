using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Layers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace PixiEditor.Helpers.Converters
{
    // TODO: Implement rebuilding only changed items instead whole tree
    public class LayersToStructuredLayersConverter
        : MultiValueMarkupConverter
    {
        private static StructuredLayerTree cachedTree;
        private List<Guid> lastLayerGuids = new List<Guid>();
        private IList<Layer> lastLayers = new List<Layer>();
        private WpfObservableRangeCollection<GuidStructureItem> lastStructure = new WpfObservableRangeCollection<GuidStructureItem>();

        public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is WpfObservableRangeCollection<Layer> layers && values[1] is LayerStructure structure)
            {
                if (cachedTree == null)
                {
                    cachedTree = new StructuredLayerTree(layers, structure);
                }

                if (TryFindStructureDifferences(structure) ||
                    lastLayerGuids.Count != layers.Count ||
                    LayerOrderIsDifferent(layers) ||
                    LayersAreDifferentObjects(layers, lastLayers))
                {
                    cachedTree = new StructuredLayerTree(layers, structure);
                    lastLayers = layers;
                    lastLayerGuids = layers.Select(x => x.GuidValue).ToList();
                    lastStructure = structure.CloneGroups();
                }

                return cachedTree.RootDirectoryItems;
            }

            return DependencyProperty.UnsetValue;
        }

        public override object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new ArgumentException("Value is not a StructuredLayerTree");
        }

        private bool LayerOrderIsDifferent(IList<Layer> layers)
        {
            var guids = layers.Select(x => x.GuidValue).ToArray();
            return !guids.SequenceEqual(lastLayerGuids);
        }

        /// <summary>
        /// This should trigger if you open and close the same files twice.
        /// Even though the layers are technically the same, having two different objects screws things up down the line.
        /// </summary>
        private bool LayersAreDifferentObjects(IList<Layer> layers, IList<Layer> lastLayers)
        {
            for (int i = 0; i < layers.Count; i++)
            {
                if (layers[i] != lastLayers[i])
                    return true;
            }
            return false;
        }

        private bool TryFindStructureDifferences(LayerStructure structure)
        {
            bool structureModified = false;

            if (lastStructure.Count != structure.Groups.Count)
            {
                return true;
            }


            foreach (GuidStructureItem treeItem in lastStructure)
            {
                var matchingGroup = structure.Groups.FirstOrDefault(x => x.GroupGuid == treeItem.GroupGuid);
                List<GuidStructureItem> changedGroups = new List<GuidStructureItem>();
                if (matchingGroup == null || StructureMismatch(treeItem, matchingGroup))
                {
                    structureModified = true;
                }

            }

            return structureModified;
        }

        private bool StructureMismatch(GuidStructureItem first, GuidStructureItem second)
        {
            bool rootMismatch = first.EndLayerGuid != second.EndLayerGuid || first.StartLayerGuid != second.StartLayerGuid || first.IsVisible != second.IsVisible || first.IsExpanded != second.IsExpanded || first.Opacity != second.Opacity || first.Subgroups.Count != second.Subgroups.Count || second.Name != first.Name;

            if (!rootMismatch && first.Subgroups.Count > 0)
            {
                for (int i = 0; i < first.Subgroups.Count; i++)
                {
                    if (StructureMismatch(first.Subgroups[i], second.Subgroups[i]))
                    {
                        return true;
                    }
                }
            }
            return rootMismatch;
        }
    }
}