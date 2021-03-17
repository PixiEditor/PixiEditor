using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PixiEditor.Models.Layers
{
    public class LayerStructure
    {
        public ObservableCollection<GuidStructureItem> Folders { get; set; }

        public GuidStructureItem GetFolderByGuid(Guid folderGuid)
        {
            return GetFolderByGuid(folderGuid, Folders);
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

        public LayerStructure(ObservableCollection<GuidStructureItem> items)
        {
            Folders = items;
        }

        public LayerStructure()
        {
            Folders = new ObservableCollection<GuidStructureItem>();
        }
    }
}