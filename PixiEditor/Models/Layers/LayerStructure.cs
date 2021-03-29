using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PixiEditor.Models.DataHolders;

namespace PixiEditor.Models.Layers
{
    public class LayerStructure
    {
        public Document Owner { get; set; }

        public ObservableCollection<GuidStructureItem> Folders { get; set; }

        public GuidStructureItem GetFolderByGuid(Guid? folderGuid)
        {
            return GetFolderByGuid(folderGuid, Folders);
        }

        public GuidStructureItem GetFolderByLayer(Guid layerGuid)
        {
            return GetFolderByLayer(layerGuid, Folders);
        }

#nullable enable
        public void MoveFolder(Guid folderGuid, GuidStructureItem? parentFolder, int newIndex)
        {
            var folder = GetFolderByGuid(folderGuid);
            bool reverseOrder = true;
            int folderTopIndex = Owner.Layers.IndexOf(Owner.Layers.First(x => x.LayerGuid == folder.EndLayerGuid));
            int folderBottomIndex = Owner.Layers.IndexOf(Owner.Layers.First(x => x.LayerGuid == folder.StartLayerGuid));

            int difference = newIndex - folderTopIndex;

            if (newIndex < folderTopIndex)
            {
                reverseOrder = false;
                difference = newIndex - folderBottomIndex;
            }

            if (difference == 0)
            {
                return;
            }

            PreMoveReassignBounds(parentFolder, folder);

            List<Guid> layersInOrder = GetLayersInOrder(new FolderData(folderTopIndex, folderBottomIndex));

            MoveLayersInFolder(layersInOrder, difference, reverseOrder);
        }

        public void PostMoveReassignBounds(GuidStructureItem? parentFolder, Guid layerGuid)
        {
            if (parentFolder != null)
            {
                int layerIndex = Owner.Layers.IndexOf(Owner.Layers.First(x => x.LayerGuid == layerGuid));

                int folderTopIndex = Owner.Layers.IndexOf(Owner.Layers.First(x => x.LayerGuid == parentFolder.EndLayerGuid));
                int folderBottomIndex = Owner.Layers.IndexOf(Owner.Layers.First(x => x.LayerGuid == parentFolder.StartLayerGuid));

                int finalTopIndex = Math.Max(folderTopIndex, layerIndex);
                int finalBottomIndex = Math.Min(folderBottomIndex, layerIndex);

                Guid? topBoundLayer = FindBoundLayer(layerGuid, finalTopIndex, finalBottomIndex, false);
                Guid? bottomBoundLayer = FindBoundLayer(layerGuid, finalTopIndex, finalBottomIndex, true);

                if (topBoundLayer == parentFolder.EndLayerGuid)
                {
                    parentFolder.EndLayerGuid = layerGuid;
                }

                if (bottomBoundLayer == parentFolder.StartLayerGuid)
                {
                    parentFolder.StartLayerGuid = layerGuid;
                }
            }
        }

        public void PostMoveReassignBounds(GuidStructureItem? parentFolder, GuidStructureItem folder)
        {
            if (parentFolder != null)
            {
                if (folder.StartLayerGuid != null && folder.EndLayerGuid != null)
                {
                    int folderTopIndex = Owner.Layers.IndexOf(Owner.Layers.First(x => x.LayerGuid == folder.EndLayerGuid));
                    int folderBottomIndex = Owner.Layers.IndexOf(Owner.Layers.First(x => x.LayerGuid == folder.StartLayerGuid));

                    int parentFolderTopIndex = Owner.Layers.IndexOf(Owner.Layers.First(x => x.LayerGuid == parentFolder.EndLayerGuid));
                    int parentFolderBottomIndex = Owner.Layers.IndexOf(Owner.Layers.First(x => x.LayerGuid == parentFolder.StartLayerGuid));

                    int finalTopIndex = Math.Max(folderTopIndex, parentFolderTopIndex);
                    int finalBottomIndex = Math.Min(folderBottomIndex, parentFolderBottomIndex);

                    Guid? topBoundLayer = FindBoundLayer((Guid)folder.StartLayerGuid, finalTopIndex, finalBottomIndex, false);
                    Guid? bottomBoundLayer = FindBoundLayer((Guid)folder.EndLayerGuid, finalTopIndex, finalBottomIndex, true);

                    if (topBoundLayer == parentFolder.EndLayerGuid)
                    {
                        parentFolder.EndLayerGuid = folder.EndLayerGuid;
                    }

                    if (bottomBoundLayer == parentFolder.StartLayerGuid)
                    {
                        parentFolder.StartLayerGuid = folder.StartLayerGuid;
                    }
                }
            }
        }

        public void PreMoveReassignBounds(GuidStructureItem? parentFolder, Guid layer)
        {
            if (parentFolder != null)
            {
                if (parentFolder.EndLayerGuid == layer)
                {
                    parentFolder.EndLayerGuid = FindBoundLayer(parentFolder, layer, false);
                }

                if (parentFolder.StartLayerGuid == layer)
                {
                    parentFolder.StartLayerGuid = FindBoundLayer(parentFolder, layer, true);
                }
            }
        }

        private void PreMoveReassignBounds(GuidStructureItem? parentFolder, GuidStructureItem folder)
        {
            if (parentFolder != null)
            {
                if (folder.EndLayerGuid == parentFolder.EndLayerGuid && folder.StartLayerGuid != null)
                {
                    parentFolder.EndLayerGuid = FindBoundLayer(parentFolder, (Guid)folder.StartLayerGuid, false);
                }

                if (folder.StartLayerGuid == parentFolder.StartLayerGuid && folder.EndLayerGuid != null)
                {
                    parentFolder.StartLayerGuid = FindBoundLayer(parentFolder, (Guid)folder.EndLayerGuid, true);
                }
            }
        }

        private Guid? FindBoundLayer(Guid layerGuid, int parentFolderTopIndex, int parentFolderBottomIndex, bool above)
        {
            return GetNextLayerGuid(
                   layerGuid,
                   GetLayersInOrder(new FolderData(parentFolderTopIndex, parentFolderBottomIndex)),
                   above);
        }

        private Guid? FindBoundLayer(GuidStructureItem parentFolder, Guid layerGuid, bool above)
        {
            int parentFolderTopIndex = Owner.Layers.IndexOf(Owner.Layers.First(x => x.LayerGuid == parentFolder.EndLayerGuid));
            int parentFolderBottomIndex = Owner.Layers.IndexOf(Owner.Layers.First(x => x.LayerGuid == parentFolder.StartLayerGuid));

            return FindBoundLayer(layerGuid, parentFolderTopIndex, parentFolderBottomIndex, above);
        }

        private Guid? GetNextLayerGuid(Guid? layer, List<Guid> allLayers, bool above)
        {
            if (layer == null)
            {
                return null;
            }

            int indexOfLayer = allLayers.IndexOf(layer.Value);

            int modifier = above ? 1 : -1;

            int newIndex = indexOfLayer + modifier;

            if (newIndex < 0 || newIndex >= allLayers.Count)
            {
                return null;
            }

            return allLayers[newIndex];
        }

        private void MoveLayersInFolder(List<Guid> layers, int moveBy, bool reverseOrder)
        {
            List<Guid> layerGuids = reverseOrder ? layers.Reverse<Guid>().ToList() : layers;

            for (int i = 0; i < layers.Count; i++)
            {
                Guid layerGuid = layerGuids[i];
                var layer = Owner.Layers.First(x => x.LayerGuid == layerGuid);
                int layerIndex = Owner.Layers.IndexOf(layer);
                Owner.Layers.Move(layerIndex, layerIndex + moveBy);
            }
        }

        private List<Guid> GetLayersInOrder(FolderData folder)
        {
            List<Guid> layerGuids = new ();
            int minIndex = folder.BottomIndex;
            int maxIndex = folder.TopIndex;

            for (int i = minIndex; i <= maxIndex; i++)
            {
                layerGuids.Add(Owner.Layers[i].LayerGuid);
            }

            return layerGuids;
        }

#nullable disable
        private GuidStructureItem GetFolderByLayer(Guid layerGuid, IEnumerable<GuidStructureItem> folders)
        {
            foreach (var folder in folders)
            {
                int topIndex = Owner.Layers.IndexOf(Owner.Layers.First(x => x.LayerGuid == folder.EndLayerGuid));
                int bottomIndex = Owner.Layers.IndexOf(Owner.Layers.First(x => x.LayerGuid == folder.StartLayerGuid));
                var layers = GetLayersInOrder(new FolderData(topIndex, bottomIndex));
                if (layers.Contains(layerGuid))
                {
                    return folder;
                }

                if (folder.Subfolders.Count > 0)
                {
                    return GetFolderByLayer(layerGuid, folder.Subfolders);
                }
            }

            return null;
        }

        private GuidStructureItem GetFolderByGuid(Guid? folderGuid, IEnumerable<GuidStructureItem> folders)
        {
            foreach (var folder in folders)
            {
                if (folder.FolderGuid == folderGuid)
                {
                    return folder;
                }

                if (folder.Subfolders.Count > 0)
                {
                    return GetFolderByGuid(folderGuid, folder.Subfolders);
                }
            }

            return null;
        }

        public LayerStructure(ObservableCollection<GuidStructureItem> items, Document owner)
        {
            Folders = items;
            Owner = owner;
        }

        public LayerStructure(Document owner)
        {
            Folders = new ObservableCollection<GuidStructureItem>();
            Owner = owner;
        }
    }
}