using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using PixiEditor.Helpers;
using PixiEditor.Models.DataHolders;

namespace PixiEditor.Models.Layers
{
    public class LayerStructure
    {
        public event EventHandler LayerStructureChanged;

        public Document Owner { get; set; }

        public ObservableCollection<GuidStructureItem> Groups { get; set; }

        public static bool GroupContainsOnlyLayer(Layer layer, GuidStructureItem layerGroup)
        {
            return layerGroup != null && layerGroup.Subgroups.Count == 0 && layerGroup.StartLayerGuid == layer.LayerGuid && layerGroup.EndLayerGuid == layer.LayerGuid;
        }

        public GuidStructureItem GetGroupByGuid(Guid? groupGuid)
        {
            return GetGroupByGuid(groupGuid, Groups);
        }

        public GuidStructureItem GetGroupByLayer(Guid layerGuid)
        {
            return GetGroupByLayer(layerGuid, Groups);
        }

        public void AddNewGroup(string groupName, Guid childLayer)
        {
            var parent = GetGroupByLayer(childLayer);
            GuidStructureItem group = new (groupName, childLayer);
            if (parent == null)
            {
                Groups.Add(group);
            }
            else
            {
                group.Parent = parent;
                parent.Subgroups.Add(group);
            }

            group.GroupsChanged += Group_GroupsChanged;

            LayerStructureChanged?.Invoke(this, EventArgs.Empty);
        }


#nullable enable
        public void MoveGroup(Guid groupGuid, GuidStructureItem? parentGroup, int newIndex)
        {
            var group = GetGroupByGuid(groupGuid);
            bool reverseOrder = true;
            int groupTopIndex = Owner.Layers.IndexOf(Owner.Layers.First(x => x.LayerGuid == group.EndLayerGuid));
            int groupBottomIndex = Owner.Layers.IndexOf(Owner.Layers.First(x => x.LayerGuid == group.StartLayerGuid));

            int difference = newIndex - groupTopIndex;

            if (newIndex < groupTopIndex)
            {
                reverseOrder = false;
                difference = newIndex - groupBottomIndex;
            }

            if (difference == 0)
            {
                return;
            }

            PreMoveReassignBounds(parentGroup, group);

            List<Guid> layersInOrder = GetLayersInOrder(new FolderData(groupTopIndex, groupBottomIndex));

            MoveLayersInGroup(layersInOrder, difference, reverseOrder);

            LayerStructureChanged?.Invoke(this, EventArgs.Empty);
        }

        public void PreMoveReassignBounds(GuidStructureItem? parentGroup, Guid layer)
        {
            if (parentGroup != null)
            {
                Guid? oldStart = parentGroup.StartLayerGuid;
                Guid? oldEnd = parentGroup.EndLayerGuid;
                GuidStructureItem parentOfParent = parentGroup.Parent;
                if (parentGroup.Subgroups.Count == 0 && parentGroup.StartLayerGuid == layer && parentGroup.EndLayerGuid == layer)
                {
                    RemoveGroup(parentGroup);
                }
                else
                {
                    if (parentGroup.EndLayerGuid == layer)
                    {
                        parentGroup.EndLayerGuid = FindBoundLayer(parentGroup, layer, false);
                    }

                    if (parentGroup.StartLayerGuid == layer)
                    {
                        parentGroup.StartLayerGuid = FindBoundLayer(parentGroup, layer, true);
                    }
                }

                if (parentOfParent != null)
                {
                    ApplyBoundsToParents(parentOfParent, parentGroup, oldStart, oldEnd);
                }
            }
        }

        public void PreMoveReassignBounds(GuidStructureItem? parentGroup, GuidStructureItem group)
        {
            if (parentGroup != null)
            {
                Guid? oldStart = parentGroup.StartLayerGuid;
                Guid? oldEnd = parentGroup.EndLayerGuid;
                if (group.EndLayerGuid == parentGroup.EndLayerGuid && group.StartLayerGuid != null)
                {
                    parentGroup.EndLayerGuid = FindBoundLayer(parentGroup, (Guid)group.StartLayerGuid, false);
                }

                if (group.StartLayerGuid == parentGroup.StartLayerGuid && group.EndLayerGuid != null)
                {
                    parentGroup.StartLayerGuid = FindBoundLayer(parentGroup, (Guid)group.EndLayerGuid, true);
                }

                if (parentGroup.Parent != null)
                {
                    ApplyBoundsToParents(parentGroup.Parent, parentGroup, oldStart, oldEnd);
                }
            }
        }

        public void PostMoveReassignBounds(GuidStructureItem? parentGroup, Guid layerGuid)
        {
            if (parentGroup != null)
            {
                Guid? oldStart = parentGroup.StartLayerGuid;
                Guid? oldEnd = parentGroup.EndLayerGuid;

                int layerIndex = Owner.Layers.IndexOf(Owner.Layers.First(x => x.LayerGuid == layerGuid));

                int folderTopIndex = Owner.Layers.IndexOf(Owner.Layers.First(x => x.LayerGuid == parentGroup.EndLayerGuid));
                int folderBottomIndex = Owner.Layers.IndexOf(Owner.Layers.First(x => x.LayerGuid == parentGroup.StartLayerGuid));

                int finalTopIndex = Math.Max(folderTopIndex, layerIndex);
                int finalBottomIndex = Math.Min(folderBottomIndex, layerIndex);

                Guid? topBoundLayer = FindBoundLayer(layerGuid, finalTopIndex, finalBottomIndex, false);
                Guid? bottomBoundLayer = FindBoundLayer(layerGuid, finalTopIndex, finalBottomIndex, true);

                if (topBoundLayer == parentGroup.EndLayerGuid)
                {
                    parentGroup.EndLayerGuid = layerGuid;
                }

                if (bottomBoundLayer == parentGroup.StartLayerGuid)
                {
                    parentGroup.StartLayerGuid = layerGuid;
                }

                if (parentGroup.Parent != null)
                {
                    ApplyBoundsToParents(parentGroup.Parent, parentGroup, oldStart, oldEnd);
                }
            }
        }

        public void PostMoveReassignBounds(GuidStructureItem? parentGroup, GuidStructureItem group)
        {
            if (parentGroup != null)
            {
                Guid? oldStart = parentGroup.StartLayerGuid;
                Guid? oldEnd = parentGroup.EndLayerGuid;

                if (group.StartLayerGuid != null && group.EndLayerGuid != null)
                {
                    int folderTopIndex = Owner.Layers.IndexOf(Owner.Layers.First(x => x.LayerGuid == group.EndLayerGuid));
                    int folderBottomIndex = Owner.Layers.IndexOf(Owner.Layers.First(x => x.LayerGuid == group.StartLayerGuid));

                    int parentFolderTopIndex = Owner.Layers.IndexOf(Owner.Layers.First(x => x.LayerGuid == parentGroup.EndLayerGuid));
                    int parentFolderBottomIndex = Owner.Layers.IndexOf(Owner.Layers.First(x => x.LayerGuid == parentGroup.StartLayerGuid));

                    int finalTopIndex = Math.Max(folderTopIndex, parentFolderTopIndex);
                    int finalBottomIndex = Math.Min(folderBottomIndex, parentFolderBottomIndex);

                    Guid? topBoundLayer = FindBoundLayer((Guid)group.StartLayerGuid, finalTopIndex, finalBottomIndex, false);
                    Guid? bottomBoundLayer = FindBoundLayer((Guid)group.EndLayerGuid, finalTopIndex, finalBottomIndex, true);

                    if (topBoundLayer == parentGroup.EndLayerGuid)
                    {
                        parentGroup.EndLayerGuid = group.EndLayerGuid;
                    }

                    if (bottomBoundLayer == parentGroup.StartLayerGuid)
                    {
                        parentGroup.StartLayerGuid = group.StartLayerGuid;
                    }
                }

                if (parentGroup.Parent != null)
                {
                    ApplyBoundsToParents(parentGroup.Parent, parentGroup, oldStart, oldEnd);
                }
            }
        }

        public void AssignParent(Guid layer, GuidStructureItem parent)
        {
            var currentParent = GetGroupByLayer(layer);
            if(currentParent != null)
            {
                PreMoveReassignBounds(currentParent, layer);
            }

            PostMoveReassignBounds(parent, layer);

            LayerStructureChanged?.Invoke(this, EventArgs.Empty);
        }

        private void Group_GroupsChanged(object sender, EventArgs e)
        {
            LayerStructureChanged?.Invoke(this, EventArgs.Empty);
        }

        private void RemoveGroup(GuidStructureItem parentFolder)
        {
            parentFolder.GroupsChanged -= Group_GroupsChanged;
            if (parentFolder.Parent == null)
            {
                Groups.Remove(parentFolder);
            }
            else
            {
                parentFolder.Parent.Subgroups.Remove(parentFolder);
            }

            LayerStructureChanged?.Invoke(this, EventArgs.Empty);
        }

        private void ApplyBoundsToParents(GuidStructureItem parent, GuidStructureItem group, Guid? oldStart, Guid? oldEnd)
        {
            Guid? parentOldStart = parent.StartLayerGuid;
            Guid? parentOldEnd = parent.EndLayerGuid;
            if (parent.StartLayerGuid == oldStart)
            {
                parent.StartLayerGuid = group.StartLayerGuid;
            }

            if (parent.EndLayerGuid == oldEnd)
            {
                parent.EndLayerGuid = group.EndLayerGuid;
            }

            if (parent.Parent != null)
            {
                ApplyBoundsToParents(parent.Parent, parent, parentOldStart, parentOldEnd);
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

        private void MoveLayersInGroup(List<Guid> layers, int moveBy, bool reverseOrder)
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
        private GuidStructureItem GetGroupByLayer(Guid layerGuid, IEnumerable<GuidStructureItem> folders)
        {
            foreach (var folder in folders)
            {
                int topIndex = Owner.Layers.IndexOf(Owner.Layers.First(x => x.LayerGuid == folder.EndLayerGuid));
                int bottomIndex = Owner.Layers.IndexOf(Owner.Layers.First(x => x.LayerGuid == folder.StartLayerGuid));
                var layers = GetLayersInOrder(new FolderData(topIndex, bottomIndex));

                if (folder.Subgroups.Count > 0)
                {
                    var group = GetGroupByLayer(layerGuid, folder.Subgroups);
                    if(group != null)
                    {
                        return group;
                    }
                }

                if (layers.Contains(layerGuid))
                {
                    return folder;
                }
            }

            return null;
        }

        private GuidStructureItem GetGroupByGuid(Guid? folderGuid, IEnumerable<GuidStructureItem> folders)
        {
            foreach (var folder in folders)
            {
                if (folder.GroupGuid == folderGuid)
                {
                    return folder;
                }

                if (folder.Subgroups.Count > 0)
                {
                    var guid = GetGroupByGuid(folderGuid, folder.Subgroups);
                    if(guid != null)
                    {
                        return guid;
                    }
                }
            }

            return null;
        }

        public LayerStructure(ObservableCollection<GuidStructureItem> items, Document owner)
        {
            Groups = items;
            Owner = owner;
        }

        public LayerStructure(Document owner)
        {
            Groups = new ObservableCollection<GuidStructureItem>();
            Owner = owner;
        }
    }
}