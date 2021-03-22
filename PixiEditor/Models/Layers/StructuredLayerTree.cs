using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PixiEditor.Helpers;
using PixiEditor.Helpers.Extensions;

namespace PixiEditor.Models.Layers
{
    public class StructuredLayerTree : NotifyableObject
    {
        private readonly List<Layer> layersInStructure = new ();

        public ObservableCollection<object> RootDirectoryItems { get; } = new ObservableCollection<object>();

        public StructuredLayerTree(ObservableCollection<Layer> layers, LayerStructure structure)
        {
            if (structure.Folders == null || structure.Folders.Count == 0)
            {
                RootDirectoryItems.AddRange(layers);
                return;
            }

            var parsedFolders = ParseFolders(structure.Folders, layers);

            RootDirectoryItems.AddRange(parsedFolders);

            RootDirectoryItems.AddRange(layers.Where(x => !layersInStructure.Contains(x)));

            for (int i = 0; i < parsedFolders.Count; i++)
            {
                RootDirectoryItems.Move(i, parsedFolders[i].DisplayIndex);
            }

            layersInStructure.Clear();
        }

        private List<LayerFolder> ParseFolders(IEnumerable<GuidStructureItem> folders, ObservableCollection<Layer> layers)
        {
            List<LayerFolder> parsedFolders = new ();
            foreach (var structureItem in folders)
            {
                parsedFolders.Add(ParseFolder(structureItem, layers));
            }

            return parsedFolders;
        }

        private LayerFolder ParseFolder(GuidStructureItem structureItem, ObservableCollection<Layer> layers)
        {
            List<Layer> structureItemLayers = new ();

            Guid[] layersInFolder = GetLayersInFolder(layers, structureItem);

            var subFolders = new List<LayerFolder>();

            if (structureItem.Subfolders.Count > 0)
            {
                subFolders = ParseFolders(structureItem.Subfolders, layers);
            }

            foreach (var guid in layersInFolder)
            {
                var layer = layers.First(x => x.LayerGuid == guid);
                if (!layersInStructure.Contains(layer))
                {
                    layersInStructure.Add(layer);
                    structureItemLayers.Add(layer);
                }
            }

            int displayIndex = layersInStructure.Min(x => layers.IndexOf(x));

            structureItemLayers.Reverse();

            LayerFolder folder = new (structureItemLayers, subFolders, structureItem.Name,
                structureItem.FolderGuid, displayIndex, displayIndex + layersInStructure.Count - 1)
            {
                IsExpanded = structureItem.IsExpanded
            };
            return folder;
        }

        private Guid[] GetLayersInFolder(ObservableCollection<Layer> layers, GuidStructureItem structureItem)
        {
            int startIndex = layers.IndexOf(layers.First(x => x.LayerGuid == structureItem.StartLayerGuid));
            int endIndex = layers.IndexOf(layers.First(x => x.LayerGuid == structureItem.EndLayerGuid));

            if (startIndex > endIndex)
            {
                Swap(ref startIndex, ref endIndex);
            }

            int len = endIndex - startIndex + 1;

            Guid[] guids = new Guid[len];

            for (int i = 0; i < len; i++)
            {
                guids[i] = layers[i + startIndex].LayerGuid;
            }

            return guids;
        }

        private static void Swap(ref int startIndex, ref int endIndex)
        {
            int tmp = startIndex;
            startIndex = endIndex;
            endIndex = tmp;
        }
    }
}