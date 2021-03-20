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

        public StructuredLayerTree(IEnumerable<Layer> layers, LayerStructure structure)
        {
            if (structure.Folders == null || structure.Folders.Count == 0)
            {
                RootDirectoryItems.AddRange(layers);
                return;
            }

            RootDirectoryItems.AddRange(layers.Where(x => !layersInStructure.Contains(x)));

            foreach (var folder in ParseFolders(structure.Folders, layers))
            {
                RootDirectoryItems.Insert(folder.DisplayIndex, folder);
            }

            layersInStructure.Clear();
        }

        private List<LayerFolder> ParseFolders(IEnumerable<GuidStructureItem> folders, IEnumerable<Layer> layers)
        {
            List<LayerFolder> parsedFolders = new ();
            foreach (var structureItem in folders)
            {
                parsedFolders.Add(ParseFolder(structureItem, layers));
            }

            return parsedFolders;
        }

        private LayerFolder ParseFolder(GuidStructureItem structureItem, IEnumerable<Layer> layers)
        {
            List<Layer> structureItemLayers = new ();
            foreach (var guid in structureItem.LayerGuids)
            {
                var layer = layers.First(x => x.LayerGuid == guid);
                layersInStructure.Add(layer);
                structureItemLayers.Add(layer);
            }

            var subFolders = new List<LayerFolder>();

            if (structureItem.Subfolders.Count > 0)
            {
                subFolders = ParseFolders(structureItem.Subfolders, layers);
            }

            LayerFolder folder = new (structureItemLayers, subFolders, structureItem.Name, 
                structureItem.FolderGuid, structureItem.FolderDisplayIndex)
            {
                IsExpanded = structureItem.IsExpanded
            };
            return folder;
        }
    }
}