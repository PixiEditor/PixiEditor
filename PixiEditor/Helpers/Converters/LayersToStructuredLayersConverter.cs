using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using PixiEditor.Models.Layers;

namespace PixiEditor.Helpers.Converters
{
    //TODO: Implement rebuilding only changed items instead whole tree
    public class LayersToStructuredLayersConverter : IMultiValueConverter
    {
        private static StructuredLayerTree cachedTree;
        private List<Guid> lastLayers = new List<Guid>();
        private ObservableCollection<GuidStructureItem> lastStructure = new ObservableCollection<GuidStructureItem>();

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is ObservableCollection<Layer> layers && values[1] is LayerStructure structure)
            {
                if (cachedTree == null)
                {
                    cachedTree = new StructuredLayerTree(layers, structure);
                }

                if (TryFindStructureDifferences(structure) || lastLayers.Count != layers.Count || LayerOrderIsDifferent(layers))
                {
                    cachedTree = new StructuredLayerTree(layers, structure);

                    lastLayers = layers.Select(x => x.LayerGuid).ToList();
                    lastStructure = structure.CloneGroups();
                }

                return cachedTree.RootDirectoryItems;
            }

            return new StructuredLayerTree(null, null);
        }

        private bool LayerOrderIsDifferent(IList<Layer> layers)
        {
            var guids = layers.Select(x => x.LayerGuid).ToArray();
            return !guids.SequenceEqual(lastLayers);
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
                if(matchingGroup == null || StructureMismatch(treeItem, matchingGroup))
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

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new ArgumentException("Value is not a StructuredLayerTree");
        }
    }
}