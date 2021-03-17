using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PixiEditor.Models.Layers
{
    public class GuidStructureItem
    {
        public string Name { get; set; }

        public ObservableCollection<Guid> LayerGuids { get; set; }

        public ObservableCollection<GuidStructureItem> Subfolders { get; set; }

        public GuidStructureItem(string name, IEnumerable<Guid> children, IEnumerable<GuidStructureItem> subfolders)
        {
            Name = name;
            LayerGuids = new ObservableCollection<Guid>(children);
            Subfolders = new ObservableCollection<GuidStructureItem>(subfolders);
        }
    }
}