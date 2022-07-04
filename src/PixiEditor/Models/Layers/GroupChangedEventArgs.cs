using System;
using System.Collections.Generic;

namespace PixiEditor.Models.Layers
{
    public class GroupChangedEventArgs : EventArgs
    {
        public List<GuidStructureItem> GroupsAffected { get; set; }

        public GroupChangedEventArgs(List<GuidStructureItem> groupsAffected)
        {
            GroupsAffected = groupsAffected;
        }
    }
}