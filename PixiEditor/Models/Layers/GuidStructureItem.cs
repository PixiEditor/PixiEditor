using System;
using System.Collections.ObjectModel;

namespace PixiEditor.Models.Layers
{
    public class GuidStructureItem
    {
        public ObservableCollection<Guid> Children { get; set; }

        public GuidStructureItem(ObservableCollection<Guid> children)
        {
            Children = children;
        }
    }
}