using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixiEditor.Models.Layers
{
    public record FolderData
    {
        public int TopIndex { get; set; }
        public int BottomIndex { get; set; }

        public FolderData(int topIndex, int bottomIndex)
        {
            TopIndex = topIndex;
            BottomIndex = bottomIndex;
        }
    }
}