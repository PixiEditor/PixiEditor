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

            int difference;

            //if (newIndex > oldFolderActualIndex)
            //{
            //    difference = indexToApply - oldFolderActualIndex;
            //}
            //else
            //{
            //    reverseOrder = false;
            //    difference = newIndex - oldFolderDisplayIndex;
            //}

            //MoveLayersInFolder(folder, difference, reverseOrder);

            //if (folder.Parent == null)
            //{
            //    Folders.Remove(folder);
            //}

            //if (parentFolder == null && !Folders.Contains(folder))
            //{
            //    Folders.Add(folder);
            //}
            //else if (parentFolder != null)
            //{
            //    parentFolder.Subfolders.Add(folder);
            //}
        }

        private void MoveLayersInFolder(GuidStructureItem folder, int moveBy, bool reverseOrder)
        {
            //List<Guid> layersInOrder = GetLayersInOrder(folder);

            //List<Guid> layerGuids = reverseOrder ? layersInOrder.Reverse<Guid>().ToList() : layersInOrder;

            //for (int i = 0; i < layersInOrder.Count; i++)
            //{
            //    Guid layerGuid = layerGuids[i];
            //    var layer = Owner.Layers.First(x => x.LayerGuid == layerGuid);
            //    int layerIndex = Owner.Layers.IndexOf(layer);
            //    Owner.Layers.Move(layerIndex, layerIndex + moveBy);
            //}
        }

        //private List<Guid> GetLayersInOrder(GuidStructureItem folder)
        //{
        //    List<Guid> layerGuids = new ();
        //    int minIndex = Owner.Layers.folder.FolderDisplayIndex;
        //    int maxIndex = folder.ActualIndex;

        //    for (int i = minIndex; i <= maxIndex; i++)
        //    {
        //        layerGuids.Add(Owner.Layers[i].LayerGuid);
        //    }

        //    return layerGuids;
        //}

#nullable disable
        private GuidStructureItem GetFolderByLayer(Guid layerGuid, IEnumerable<GuidStructureItem> folders)
        {
            //foreach (var folder in folders)
            //{
            //    if (folder.LayerGuids.Contains(layerGuid))
            //    {
            //        return folder;
            //    }

            //    if (folder.Subfolders.Count > 0)
            //    {
            //        return GetFolderByLayer(layerGuid, folder.Subfolders);
            //    }
            //}

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