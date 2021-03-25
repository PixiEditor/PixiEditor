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

        public GuidStructureItem GetFolderByGuid(Guid folderGuid)
        {
            return GetFolderByGuid(folderGuid, Folders);
        }

        /// <summary>
        /// Moves layer from folder if already exist to new one, or removes completely if specified folderGuid is null;
        /// </summary>
        /// <param name="layerGuid">Guid of layer to move.</param>
        /// <param name="folderGuid">Folder guid to move layer to, set to null to remove completely.</param>
        public void MoveLayerToFolder(Guid layerGuid, Guid? folderGuid)
        {
            GuidStructureItem oldFolder = GetFolderByLayer(layerGuid);
            //if (oldFolder != null)
            //{
            //    oldFolder.LayerGuids.Remove(layerGuid);
            //}

            //if (folderGuid != null)
            //{
            //    var folder = GetFolderByGuid((Guid)folderGuid);
            //    folder.LayerGuids.Add(layerGuid);
            //}
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

            int difference;

            if (newIndex >= folderTopIndex)
            {
                difference = newIndex - folderTopIndex;
            }
            else
            {
                reverseOrder = false;
                difference = newIndex - folderBottomIndex;
            }

            if (difference == 0)
            {
                return;
            }

            bool parentBoundsReassigned = ReassignBounds(parentFolder, folder);

            List<Guid> layersInOrder = GetLayersInOrder(new FolderData(folderTopIndex, folderBottomIndex));

            Guid oldLayerAtIndex = Owner.Layers[newIndex].LayerGuid;

            MoveLayersInFolder(layersInOrder, difference, reverseOrder);
        }

        public bool ReassignBounds(GuidStructureItem? parentFolder, GuidStructureItem folder)
        {
            if (parentFolder != null)
            {
                bool reassigned = false;
                if (folder.EndLayerGuid == parentFolder.EndLayerGuid && folder.StartLayerGuid != null)
                {
                    parentFolder.EndLayerGuid = FindBoundLayer(parentFolder, (Guid)folder.StartLayerGuid, false);
                    reassigned = true;
                }

                if (folder.StartLayerGuid == parentFolder.StartLayerGuid && folder.EndLayerGuid != null)
                {
                    parentFolder.StartLayerGuid = FindBoundLayer(parentFolder, (Guid)folder.EndLayerGuid, true);
                    reassigned = true;
                }

                return reassigned;
            }

            return false;
        }

        private Guid? FindBoundLayer(GuidStructureItem parentFolder, Guid oldLayerGuid, bool above)
        {
            int parentFolderTopIndex = Owner.Layers.IndexOf(Owner.Layers.First(x => x.LayerGuid == parentFolder.EndLayerGuid));
            int parentFolderBottomIndex = Owner.Layers.IndexOf(Owner.Layers.First(x => x.LayerGuid == parentFolder.StartLayerGuid));

            return GetNextLayerGuid(
                    oldLayerGuid,
                    GetLayersInOrder(new FolderData(parentFolderTopIndex, parentFolderBottomIndex)),
                    above);
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

        private GuidStructureItem GetFolderByGuid(Guid folderGuid, IEnumerable<GuidStructureItem> folders)
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