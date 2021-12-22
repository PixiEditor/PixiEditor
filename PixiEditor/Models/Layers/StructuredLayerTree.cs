using PixiEditor.Helpers;
using PixiEditor.Models.DataHolders;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PixiEditor.Models.Layers
{
    public class StructuredLayerTree : NotifyableObject
    {
        private List<Guid> layersInStructure = new();

        public DataHolders.ObservableCollection<IHasGuid> RootDirectoryItems { get; } = new DataHolders.ObservableCollection<IHasGuid>();

        private static void Swap(ref int startIndex, ref int endIndex)
        {
            (startIndex, endIndex) = (endIndex, startIndex);
        }

        public StructuredLayerTree(DataHolders.ObservableCollection<Layer> layers, LayerStructure structure)
        {
            if (layers == null || structure == null)
            {
                return;
            }

            if (structure.Groups == null || structure.Groups.Count == 0)
            {
                RootDirectoryItems.AddRange(layers);
                return;
            }

            var parsedFolders = ParseFolders(structure.Groups, layers);

            parsedFolders = parsedFolders.OrderBy(x => x.DisplayIndex).ToList();

            PlaceItems(parsedFolders, layers);

            layersInStructure.Clear();
        }

        private void PlaceItems(List<LayerGroup> parsedFolders, System.Collections.ObjectModel.ObservableCollection<Layer> layers)
        {
            LayerGroup currentFolder = null;
            List<LayerGroup> groupsAtIndex = new();
            Stack<LayerGroup> unfinishedFolders = new();

            for (int i = 0; i < layers.Count; i++)
            {
                if (currentFolder != null && layers[i].GuidValue == currentFolder.StructureData.EndLayerGuid)
                {
                    if (unfinishedFolders.Count > 0)
                    {
                        currentFolder = unfinishedFolders.Pop();
                    }
                    else
                    {
                        currentFolder = null;
                    }

                    continue;
                }

                AssignGroup(parsedFolders, layers, ref currentFolder, ref groupsAtIndex, unfinishedFolders, i);

                if (currentFolder == null && !layersInStructure.Contains(layers[i].GuidValue))
                {
                    RootDirectoryItems.Add(layers[i]);
                }
                else if (!RootDirectoryItems.Contains(currentFolder))
                {
                    RootDirectoryItems.AddRange(groupsAtIndex.Where(x => !RootDirectoryItems.Contains(x)));
                }
            }
        }

        private void AssignGroup(List<LayerGroup> parsedFolders, System.Collections.ObjectModel.ObservableCollection<Layer> layers, ref LayerGroup currentFolder, ref List<LayerGroup> groupsAtIndex, Stack<LayerGroup> unfinishedFolders, int i)
        {
            if (parsedFolders.Any(x => x.StructureData.StartLayerGuid == layers[i].GuidValue))
            {
                groupsAtIndex = parsedFolders.Where(x => x.StructureData.StartLayerGuid == layers[i].GuidValue).ToList();
                for (int j = 0; j < groupsAtIndex.Count; j++)
                {
                    LayerGroup group = groupsAtIndex[j];

                    if (currentFolder != null)
                    {
                        unfinishedFolders.Push(currentFolder);
                    }

                    groupsAtIndex[j] = parsedFolders.First(x => x.StructureData.StartLayerGuid == layers[i].GuidValue);
                    groupsAtIndex[j].DisplayIndex = RootDirectoryItems.Count;
                    groupsAtIndex[j].TopIndex = CalculateTopIndex(group.DisplayIndex, group.StructureData, layers);
                    if (groupsAtIndex[j].StructureData.EndLayerGuid != layers[i].GuidValue)
                    {
                        currentFolder = groupsAtIndex[j];
                    }
                }
            }
        }

        private int CalculateTopIndex(int displayIndex, GuidStructureItem structureData, System.Collections.ObjectModel.ObservableCollection<Layer> layers)
        {
            var endLayer = layers.FirstOrDefault(x => x.GuidValue == structureData.EndLayerGuid);
            var bottomLayer = layers.FirstOrDefault(x => x.GuidValue == structureData.StartLayerGuid);
            int originalTopIndex = 0;
            int originalBottomIndex = 0;
            if (endLayer != null)
            {
                originalTopIndex = layers.IndexOf(endLayer);
            }
            if (bottomLayer != null)
            {
                originalBottomIndex = layers.IndexOf(bottomLayer);
            }

            return displayIndex + (originalTopIndex - originalBottomIndex);
        }

        private List<LayerGroup> ParseFolders(IEnumerable<GuidStructureItem> folders, System.Collections.ObjectModel.ObservableCollection<Layer> layers)
        {
            List<LayerGroup> parsedFolders = new();
            foreach (var structureItem in folders)
            {
                parsedFolders.Add(ParseFolder(structureItem, layers));
            }

            return parsedFolders;
        }

        private LayerGroup ParseFolder(GuidStructureItem structureItem, System.Collections.ObjectModel.ObservableCollection<Layer> layers)
        {
            List<Layer> structureItemLayers = new();

            Guid[] layersInFolder = GetLayersInGroup(layers, structureItem);

            var subFolders = new List<LayerGroup>();

            if (structureItem.Subgroups.Count > 0)
            {
                subFolders = ParseFolders(structureItem.Subgroups, layers);
            }

            foreach (var guid in layersInFolder)
            {
                var layer = layers.FirstOrDefault(x => x.GuidValue == guid);
                if (layer != null)
                {
                    if (!layersInStructure.Contains(layer.GuidValue))
                    {
                        layersInStructure.Add(layer.GuidValue);
                        structureItemLayers.Add(layer);
                    }
                }
            }

            int displayIndex = layersInFolder.Length > 0 ? layers.IndexOf(layers.First(x => x.GuidValue == structureItem.StartLayerGuid)) : 0;

            structureItemLayers.Reverse();

            LayerGroup folder = new(structureItemLayers, subFolders, structureItem.Name,
                structureItem.GroupGuid, displayIndex, displayIndex + structureItemLayers.Count - 1, structureItem)
            {
                IsExpanded = structureItem.IsExpanded,
                IsRenaming = structureItem.IsRenaming
            };
            return folder;
        }

        private Guid[] GetLayersInGroup(System.Collections.ObjectModel.ObservableCollection<Layer> layers, GuidStructureItem structureItem)
        {
            var startLayer = layers.FirstOrDefault(x => x.GuidValue == structureItem.StartLayerGuid);
            var endLayer = layers.FirstOrDefault(x => x.GuidValue == structureItem.EndLayerGuid);

            if (startLayer == null || endLayer == null)
            {
                return Array.Empty<Guid>();
            }

            int startIndex = layers.IndexOf(startLayer);
            int endIndex = layers.IndexOf(endLayer);

            if (startIndex > endIndex)
            {
                Swap(ref startIndex, ref endIndex);
            }

            int len = endIndex - startIndex + 1;

            Guid[] guids = new Guid[len];

            for (int i = 0; i < len; i++)
            {
                guids[i] = layers[i + startIndex].GuidValue;
            }

            return guids;
        }
    }
}
