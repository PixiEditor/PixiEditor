using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PixiEditor.Helpers;
using PixiEditor.Models.DataHolders;

namespace PixiEditor.Models.Layers
{
    // Notice for further developemnt. Remember to expose only GroupData classes if you want to modify this LayerStructure groups
    // LayerStructure should figure out the GuidStructureItem from its internal data. This will ensure that data will
    // modify correctly.
    // You should pass GuidStructureItem for operating on protected and private methods for faster data manipulation.

    /// <summary>
    /// Class containing layer groups structure and methods to operate on it.
    /// </summary>
    public class LayerStructure
    {
        public event EventHandler<LayerStructureChangedEventArgs> LayerStructureChanged;

        public ObservableCollection<GuidStructureItem> Groups { get; set; }

        private Document Owner { get; }

        /// <summary>
        /// Checks whenever group contains only single layer and none subgroups.
        /// </summary>
        /// <param name="layerGuid">Guid of layer to check.</param>
        /// <param name="layerGroup">Group to check.</param>
        /// <returns>True if group contains single layer (EndLayerGuid and StartLayerGuid == layerGuid) and none subgroups.</returns>
        public static bool GroupContainsOnlyLayer(Guid layerGuid, GuidStructureItem layerGroup)
        {
            return layerGroup != null && layerGroup.Subgroups.Count == 0 && layerGroup.StartLayerGuid == layerGuid && layerGroup.EndLayerGuid == layerGuid;
        }

        /// <summary>
        /// Deep clones groups.
        /// </summary>
        /// <param name="groups">Groups to clone.</param>
        /// <returns>ObservableCollection with cloned groups.</returns>
        public static ObservableCollection<GuidStructureItem> CloneGroups(ObservableCollection<GuidStructureItem> groups)
        {
            ObservableCollection<GuidStructureItem> outputGroups = new();
            foreach (var group in groups.ToArray())
            {
                outputGroups.Add(group.CloneGroup());
            }

            return outputGroups;
        }

        /// <summary>
        /// Finds <see cref="GuidStructureItem"/> (Group) by it's guid.
        /// </summary>
        /// <param name="groupGuid">Guid of group.</param>
        /// <returns><see cref="GuidStructureItem"/> if group was found or null if not.</returns>
        public GuidStructureItem GetGroupByGuid(Guid? groupGuid)
        {
            return GetGroupByGuid(groupGuid, Groups);
        }

        /// <summary>
        ///  Finds parent group by layer guid.
        /// </summary>
        /// <param name="layerGuid">Guid of group to check.</param>
        /// <returns><see cref="GuidStructureItem"/>if parent group was found or null if not.</returns>
        public GuidStructureItem GetGroupByLayer(Guid layerGuid)
        {
            return GetGroupByLayer(layerGuid, Groups);
        }

        public ObservableCollection<GuidStructureItem> CloneGroups()
        {
            return CloneGroups(Groups);
        }

        // This will allow to add new group with multiple layers and groups at once. Not working well, todo fix
        /*public GuidStructureItem AddNewGroup(string groupName, IEnumerable<Layer> layers, Guid activeLayer)
        {
            var activeLayerParent = GetGroupByLayer(activeLayer);

            List<GuidStructureItem> sameLevelGroups = new List<GuidStructureItem>();

            var group = AddNewGroup(groupName, activeLayer);

            if (activeLayerParent == null)
            {
                sameLevelGroups.AddRange(Groups);
            }
            else
            {
                sameLevelGroups.AddRange(activeLayerParent.Subgroups);
            }

            sameLevelGroups.Remove(group);
            group.Subgroups = new ObservableCollection<GuidStructureItem>(sameLevelGroups);

            sameLevelGroups = new(sameLevelGroups.Where(x => IsChildOf(activeLayer, x)));

            Guid lastLayer = activeLayer;

            foreach (var layer in layers)
            {
                if (layer.LayerGuid == activeLayer)
                {
                    continue;
                }

                Owner.MoveLayerInStructure(layer.LayerGuid, lastLayer, false);
                lastLayer = layer.LayerGuid;
            }

            return group;
        }*/

        /// <summary>
        /// Adds a new group to layer structure taking into consideration nesting. Invokes LayerStructureChanged event.
        /// </summary>
        /// <param name="groupName">Name of a group.</param>
        /// <param name="childLayer">Child layer of a new group.</param>
        /// <returns>Newly created group (<see cref="GuidStructureItem"/>).</returns>
        public GuidStructureItem AddNewGroup(string groupName, Guid childLayer)
        {
            var parent = GetGroupByLayer(childLayer);
            GuidStructureItem group = new(groupName, childLayer) { IsExpanded = true };
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

            LayerStructureChanged?.Invoke(this, new LayerStructureChangedEventArgs(childLayer));
            return group;
        }

        public GuidStructureItem AddNewGroup(string groupName, GuidStructureItem childGroup)
        {
            if (childGroup == null)
            {
                throw new ArgumentException("Child group can't be null.");
            }
            GuidStructureItem group = new($"{childGroup.Name} (1)", childGroup.StartLayerGuid, childGroup.EndLayerGuid, new[] { childGroup }, childGroup.Parent) { IsExpanded = true };
            if (childGroup.Parent == null)
            {
                Groups.Add(group);
                Groups.Remove(childGroup);
            }
            else
            {
                childGroup.Parent.Subgroups.Add(group);
                childGroup.Parent.Subgroups.Remove(childGroup);
            }

            childGroup.Parent = group;

            group.GroupsChanged += Group_GroupsChanged;

            LayerStructureChanged?.Invoke(this, new LayerStructureChangedEventArgs(GetGroupLayerGuids(group)));
            return group;
        }

#nullable enable
        /// <summary>
        /// Moves group and it's children from one index to another. This method makes changes in <see cref="Document"/> Layers.
        /// </summary>
        /// <param name="groupGuid">Group guid to move.</param>
        /// <param name="newIndex">New group index, relative to <see cref="Document"/> Layers.</param>
        public void MoveGroup(Guid groupGuid, int newIndex)
        {
            var group = GetGroupByGuid(groupGuid);
            var parentGroup = group.Parent;
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

            List<Guid> layersInOrder = GetLayersInOrder(new GroupData(groupTopIndex, groupBottomIndex));

            MoveLayersInGroup(layersInOrder, difference, reverseOrder);

            LayerStructureChanged?.Invoke(this, new LayerStructureChangedEventArgs(layersInOrder));
        }

        /// <summary>
        /// Checks if group is nested inside parent group.
        /// </summary>
        /// <param name="group">Group to check.</param>
        /// <param name="parent">Parent of that group.</param>
        /// <returns>True if group is nested inside parent, false if not.</returns>
        public bool IsChildOf(GuidStructureItem? group, GuidStructureItem parent)
        {
            if (group == null)
            {
                return false;
            }

            foreach (var subgroup in parent.Subgroups)
            {
                if (subgroup == group)
                {
                    return true;
                }

                if (subgroup.Subgroups.Count > 0)
                {
                    if (IsChildOf(group, subgroup))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if layer is nested inside parent group.
        /// </summary>
        /// <param name="layerGuid">Layer GUID to check.</param>
        /// <param name="parent">Parent of that group.</param>
        /// <returns>True if layer is nested inside parent, false if not.</returns>
        public bool IsChildOf(Guid layerGuid, GuidStructureItem parent)
        {
            var layerParent = GetGroupByLayer(layerGuid);

            return layerParent == parent ? true : IsChildOf(layerParent, parent);
        }

        /// <summary>
        /// Reassigns (removes) group data from parent group.
        /// </summary>
        /// <param name="parentGroup">Parent group to reassign data in.</param>
        /// <param name="group">Group which data should be reassigned.</param>
        public void PreMoveReassignBounds(GroupData parentGroup, GroupData group)
        {
            PreMoveReassignBounds(GetGroupByGuid(parentGroup.GroupGuid), GetGroupByGuid(group.GroupGuid));
        }

        /// <summary>
        /// Reassigns (removes) layer data from parent group.
        /// </summary>
        /// <param name="parentGroup">Parent group to reassign data in.</param>
        /// <param name="layer">Layer which data should be reassigned.</param>
        public void PreMoveReassignBounds(GroupData parentGroup, Guid layer)
        {
            PreMoveReassignBounds(GetGroupByGuid(parentGroup.GroupGuid), layer);
        }

        /// <summary>
        /// Reassigns (adds) layer data to parent group.
        /// </summary>
        /// <param name="parentGroup">Parent group to reassign data in.</param>
        /// <param name="layerGuid">Group which data should be reassigned.</param>
        public void PostMoveReassignBounds(GroupData parentGroup, Guid layerGuid)
        {
            PostMoveReassignBounds(GetGroupByGuid(parentGroup.GroupGuid), layerGuid);
        }

        /// <summary>
        /// Reassigns (adds) group data to parent group.
        /// </summary>
        /// <param name="parentGroup">Parent group to reassign data in.</param>
        /// <param name="group">Group which data should be reassigned.</param>
        public void PostMoveReassignBounds(GroupData parentGroup, GroupData group)
        {
            PostMoveReassignBounds(GetGroupByGuid(parentGroup.GroupGuid), GetGroupByGuid(group.GroupGuid));
        }

        /// <summary>
        /// Assigns parent to a layer.
        /// </summary>
        /// <param name="layer">Layer to assign parent to.</param>
        /// <param name="parent">Parent which should be assigned. Null indicates no parent.</param>
        public void AssignParent(Guid layer, Guid? parent)
        {
            AssignParent(layer, parent.HasValue ? GetGroupByGuid(parent) : null);
        }

        /// <summary>
        /// Assigns group new parent.
        /// </summary>
        /// <param name="group">Group to assign parent</param>
        /// <param name="referenceLayerGroup">Parent of group.</param>
        public void ReassignParent(GuidStructureItem group, GuidStructureItem referenceLayerGroup)
        {
            group.Parent?.Subgroups.Remove(group);
            if (Groups.Contains(group))
            {
                Groups.Remove(group);
            }

            if (referenceLayerGroup == null)
            {
                if (!Groups.Contains(group))
                {
                    Groups.Add(group);
                    group.Parent = null;
                }
            }
            else
            {
                referenceLayerGroup.Subgroups.Add(group);
                group.Parent = referenceLayerGroup;
            }

            LayerStructureChanged?.Invoke(this, new LayerStructureChangedEventArgs(GetGroupLayerGuids(group)));
        }

        /// <summary>
        /// Gets all layers inside group, including nested groups.
        /// </summary>
        /// <param name="group">Group to get layers from.</param>
        /// <returns>List of layers.</returns>
        public List<Layer> GetGroupLayers(GuidStructureItem group)
        {
            List<Layer> layers = new();
            var layerGuids = GetGroupLayerGuids(group);
            foreach (var layerGuid in layerGuids)
            {
                layers.Add(Owner.Layers.First(x => x.LayerGuid == layerGuid));
            }

            return layers;
        }

        /// <summary>
        /// Sets parent groups IsExpanded to true.
        /// </summary>
        /// <param name="layerGuid">Guid of layer which parents will be affected.</param>
        public void ExpandParentGroups(Guid layerGuid)
        {
            var group = GetGroupByLayer(layerGuid);

            while (group != null)
            {
                group.IsExpanded = true;
                group = group.Parent;
            }
        }

        /// <summary>
        /// Sets parent groups IsExpanded to true.
        /// </summary>
        /// <param name="group">Group which parents will be affected.</param>
        public void ExpandParentGroups(GuidStructureItem group)
        {
            GuidStructureItem currentGroup = group;

            while (currentGroup != null)
            {
                currentGroup.IsExpanded = true;
                currentGroup = currentGroup.Parent;
            }
        }

        /// <summary>
        /// Gets all layers inside group, including nested groups.
        /// </summary>
        /// <param name="group">Group to get layers from.</param>
        /// <returns>List of layer guids.</returns>
        private List<Guid> GetGroupLayerGuids(GuidStructureItem group)
        {
            Layer? layerTop = Owner.Layers.FirstOrDefault(x => x.LayerGuid == group.EndLayerGuid);
            Layer? layerBottom = Owner.Layers.FirstOrDefault(x => x.LayerGuid == group.StartLayerGuid);

            if(layerTop == null || layerBottom == null)
            {
                return new List<Guid>();
            }

            int indexTop = Owner.Layers.IndexOf(layerTop);
            int indexBottom = Owner.Layers.IndexOf(layerBottom);

            return GetLayersInOrder(new GroupData(indexTop, indexBottom));
        }

        private List<Guid> GetLayersInOrder(GroupData group)
        {
            List<Guid> layerGuids = new();
            int minIndex = group.BottomIndex;
            int maxIndex = group.TopIndex;

            for (int i = minIndex; i <= maxIndex; i++)
            {
                layerGuids.Add(Owner.Layers[i].LayerGuid);
            }

            return layerGuids;
        }

        private void PreMoveReassignBounds(GuidStructureItem? parentGroup, Guid layer)
        {
            if (parentGroup != null)
            {
                Guid oldStart = parentGroup.StartLayerGuid;
                Guid oldEnd = parentGroup.EndLayerGuid;
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
                LayerStructureChanged?.Invoke(this, new LayerStructureChangedEventArgs(layer));
            }
        }

        private void PreMoveReassignBounds(GuidStructureItem? parentGroup, GuidStructureItem group)
        {
            if (parentGroup != null)
            {
                Guid oldStart = parentGroup.StartLayerGuid;
                Guid oldEnd = parentGroup.EndLayerGuid;

                if (parentGroup.Subgroups.Count == 1 && parentGroup.StartLayerGuid == group.StartLayerGuid && parentGroup.EndLayerGuid == group.EndLayerGuid)
                {
                    RemoveGroup(parentGroup);
                }
                else
                {
                    if (group.EndLayerGuid == parentGroup.EndLayerGuid)
                    {
                        parentGroup.EndLayerGuid = FindBoundLayer(parentGroup, group.StartLayerGuid, false);
                    }

                    if (group.StartLayerGuid == parentGroup.StartLayerGuid)
                    {
                        parentGroup.StartLayerGuid = FindBoundLayer(parentGroup, group.EndLayerGuid, true);
                    }
                }

                if (parentGroup.Parent != null)
                {
                    ApplyBoundsToParents(parentGroup.Parent, parentGroup, oldStart, oldEnd);
                }
            }
        }

        private void PostMoveReassignBounds(GuidStructureItem? parentGroup, Guid layerGuid)
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

                var args = new LayerStructureChangedEventArgs(layerGuid);

                if (topBoundLayer.HasValue)
                {
                    args.AffectedLayerGuids.Add(topBoundLayer.Value);
                }
                if (bottomBoundLayer.HasValue)
                {
                    args.AffectedLayerGuids.Add(bottomBoundLayer.Value);
                }

                LayerStructureChanged?.Invoke(this, args);
            }
        }

        private void PostMoveReassignBounds(GuidStructureItem? parentGroup, GuidStructureItem group)
        {
            if (parentGroup != null)
            {
                Guid oldStart = parentGroup.StartLayerGuid;
                Guid oldEnd = parentGroup.EndLayerGuid;
                int folderTopIndex = Owner.Layers.IndexOf(Owner.Layers.First(x => x.LayerGuid == group.EndLayerGuid));
                int folderBottomIndex = Owner.Layers.IndexOf(Owner.Layers.First(x => x.LayerGuid == group.StartLayerGuid));

                int parentFolderTopIndex = Owner.Layers.IndexOf(Owner.Layers.First(x => x.LayerGuid == parentGroup.EndLayerGuid));
                int parentFolderBottomIndex = Owner.Layers.IndexOf(Owner.Layers.First(x => x.LayerGuid == parentGroup.StartLayerGuid));

                int finalTopIndex = Math.Max(folderTopIndex, parentFolderTopIndex);
                int finalBottomIndex = Math.Min(folderBottomIndex, parentFolderBottomIndex);

                Guid topBoundLayer = FindBoundLayer(group.StartLayerGuid, finalTopIndex, finalBottomIndex, false);
                Guid bottomBoundLayer = FindBoundLayer(group.EndLayerGuid, finalTopIndex, finalBottomIndex, true);

                if (topBoundLayer == parentGroup.EndLayerGuid)
                {
                    parentGroup.EndLayerGuid = group.EndLayerGuid;
                }

                if (bottomBoundLayer == parentGroup.StartLayerGuid)
                {
                    parentGroup.StartLayerGuid = group.StartLayerGuid;
                }

                if (parentGroup.Parent != null)
                {
                    ApplyBoundsToParents(parentGroup.Parent, parentGroup, oldStart, oldEnd);
                }
            }
        }

        private void AssignParent(Guid layer, GuidStructureItem? parent)
        {
            var currentParent = GetGroupByLayer(layer);
            if (currentParent != null)
            {
                PreMoveReassignBounds(currentParent, layer);
            }

            PostMoveReassignBounds(parent, layer);

            LayerStructureChanged?.Invoke(this, new LayerStructureChangedEventArgs(layer));
        }

        private void Group_GroupsChanged(object sender, GroupChangedEventArgs e)
        {
            List<Guid> layersAffected = new List<Guid>();
            e.GroupsAffected.ForEach(x => layersAffected.AddRange(GetGroupLayerGuids(x)));
            LayerStructureChanged?.Invoke(this, new LayerStructureChangedEventArgs(layersAffected));
        }

        private void RemoveGroup(GuidStructureItem parentFolder)
        {
            parentFolder.GroupsChanged -= Group_GroupsChanged;

            var layerGuids = GetGroupLayerGuids(parentFolder);

            if (parentFolder.Parent == null)
            {
                Groups.Remove(parentFolder);
            }
            else
            {
                parentFolder.Parent.Subgroups.Remove(parentFolder);
            }

            LayerStructureChanged?.Invoke(this, new LayerStructureChangedEventArgs(layerGuids));

        }

        private void ApplyBoundsToParents(GuidStructureItem parent, GuidStructureItem group, Guid? oldStart, Guid? oldEnd)
        {
            Guid parentOldStart = parent.StartLayerGuid;
            Guid parentOldEnd = parent.EndLayerGuid;

            if (parent.Subgroups.Count == 0)
            {
                RemoveGroup(parent);
            }

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

        private Guid FindBoundLayer(Guid layerGuid, int parentFolderTopIndex, int parentFolderBottomIndex, bool above)
        {
            return GetNextLayerGuid(
                   layerGuid,
                   GetLayersInOrder(new GroupData(parentFolderTopIndex, parentFolderBottomIndex)),
                   above);
        }

        private Guid FindBoundLayer(GuidStructureItem parentFolder, Guid layerGuid, bool above)
        {
            int parentFolderTopIndex = Owner.Layers.IndexOf(Owner.Layers.First(x => x.LayerGuid == parentFolder.EndLayerGuid));
            int parentFolderBottomIndex = Owner.Layers.IndexOf(Owner.Layers.First(x => x.LayerGuid == parentFolder.StartLayerGuid));

            return FindBoundLayer(layerGuid, parentFolderTopIndex, parentFolderBottomIndex, above);
        }

        private static Guid GetNextLayerGuid(Guid layer, List<Guid> allLayers, bool above)
        {
            int indexOfLayer = allLayers.IndexOf(layer);

            int modifier = above ? 1 : -1;

            int newIndex = Math.Clamp(indexOfLayer + modifier, 0, allLayers.Count - 1);

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

#nullable disable
        private GuidStructureItem GetGroupByLayer(Guid layerGuid, IEnumerable<GuidStructureItem> groups)
        {
            foreach (var currentGroup in groups)
            {
                var endLayer = Owner.Layers.First(x => x.LayerGuid == currentGroup.EndLayerGuid);
                var startLayer = Owner.Layers.First(x => x.LayerGuid == currentGroup.StartLayerGuid);

                int topIndex = Owner.Layers.IndexOf(endLayer);
                int bottomIndex = Owner.Layers.IndexOf(startLayer);
                var layers = GetLayersInOrder(new GroupData(topIndex, bottomIndex));

                if (currentGroup.Subgroups.Count > 0)
                {
                    var group = GetGroupByLayer(layerGuid, currentGroup.Subgroups);
                    if (group != null)
                    {
                        return group;
                    }
                }

                if (layers.Contains(layerGuid))
                {
                    return currentGroup;
                }
            }

            return null;
        }

        private GuidStructureItem GetGroupByGuid(Guid? groupGuid, IEnumerable<GuidStructureItem> groups)
        {
            foreach (var group in groups)
            {
                if (group.GroupGuid == groupGuid)
                {
                    return group;
                }

                if (group.Subgroups.Count > 0)
                {
                    var guid = GetGroupByGuid(groupGuid, group.Subgroups);
                    if (guid != null)
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
