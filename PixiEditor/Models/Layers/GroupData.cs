using PixiEditor.Views.UserControls;
using System;

namespace PixiEditor.Models.Layers
{
    public record GroupData
    {
        public int TopIndex { get; set; }
        public int BottomIndex { get; set; }
        public Guid? GroupGuid { get; set; }

        public GroupData(Guid? groupGuid)
        {
            GroupGuid = groupGuid;
        }

        public GroupData(int topIndex, int bottomIndex)
        {
            TopIndex = topIndex;
            BottomIndex = bottomIndex;
        }
    }
}