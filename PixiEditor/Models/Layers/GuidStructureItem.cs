using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using PixiEditor.Helpers;

namespace PixiEditor.Models.Layers
{
    public class GuidStructureItem : NotifyableObject
    {
        public Guid FolderGuid { get; init; }

        private string name;

        public string Name
        {
            get => name;
            set
            {
                name = value;
                RaisePropertyChanged(nameof(Name));
            }
        }

        public ObservableCollection<Guid> LayerGuids { get; set; }

        public ObservableCollection<GuidStructureItem> Subfolders { get; set; }

        public GuidStructureItem Parent { get; set; }

        private bool isExpanded;

        public bool IsExpanded
        {
            get => isExpanded;
            set
            {
                isExpanded = value;
                RaisePropertyChanged(nameof(IsExpanded));
            }
        }

        private int actualIndex;

        public int ActualIndex
        {
            get => actualIndex;
            set
            {
                actualIndex = value;
                RaisePropertyChanged(nameof(ActualIndex));
            }
        }

        public int FolderDisplayIndex
        {
            get => ActualIndex - GetLayersCount() + 1;
        }

        public int GetLayersCount()
        {
            return GetLayersCount(this);
        }

        private int GetLayersCount(GuidStructureItem item)
        {
            if (Subfolders.Count == 0)
            {
                return LayerGuids.Count;
            }

            int itemsCount = 0;
            foreach (var subFolder in item.Subfolders)
            {
                itemsCount += GetLayersCount(subFolder);
            }

            return itemsCount + LayerGuids.Count;
        }

        public GuidStructureItem(
            string name,
            IEnumerable<Guid> children,
            IEnumerable<GuidStructureItem> subfolders,
            int index,
            GuidStructureItem parent)
        {
            Name = name;
            LayerGuids = new ObservableCollection<Guid>(children);
            Subfolders = new ObservableCollection<GuidStructureItem>(subfolders);
            FolderGuid = Guid.NewGuid();
            ActualIndex = index;
            Parent = parent;
        }

        public GuidStructureItem(string name)
        {
            Name = name;
            LayerGuids = new ObservableCollection<Guid>();
            Subfolders = new ObservableCollection<GuidStructureItem>();
            FolderGuid = Guid.NewGuid();
            ActualIndex = 0;
            Parent = null;
        }
    }
}