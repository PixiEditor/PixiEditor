using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using PixiEditor.Helpers;

namespace PixiEditor.Models.Layers
{
    [DebuggerDisplay("{Name} - {GroupGuid}")]
    public class GuidStructureItem : NotifyableObject, ICloneable
    {
        public event EventHandler<GroupChangedEventArgs> GroupsChanged;

        public Guid GroupGuid { get; init; }

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

        private Guid startLayerGuid;

        public Guid StartLayerGuid
        {
            get => startLayerGuid;
            set
            {
                startLayerGuid = value;
                RaisePropertyChanged(nameof(StartLayerGuid));
            }
        }

        private Guid endLayerGuid;

        public Guid EndLayerGuid
        {
            get => endLayerGuid;
            set
            {
                endLayerGuid = value;
                RaisePropertyChanged(nameof(EndLayerGuid));
            }
        }

        public ObservableCollection<GuidStructureItem> Subgroups { get; set; } = new ObservableCollection<GuidStructureItem>();

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

        private bool isRenaming = false;

        public bool IsRenaming
        {
            get => isRenaming;
            set => SetProperty(ref isRenaming, value);
        }

        private bool isVisible = true;

        public bool IsVisible
        {
            get => isVisible;
            set => SetProperty(ref isVisible, value);
        }

        private float opacity = 1;

        public float Opacity
        {
            get => opacity;
            set => SetProperty(ref opacity, value);
        }

        public GuidStructureItem(
            string name,
            Guid startLayerGuid,
            Guid endLayerGuid,
            IEnumerable<GuidStructureItem> subgroups,
            GuidStructureItem parent)
        {
            Name = name;
            Subgroups = new ObservableCollection<GuidStructureItem>(subgroups);
            GroupGuid = Guid.NewGuid();
            Parent = parent;
            StartLayerGuid = startLayerGuid;
            EndLayerGuid = endLayerGuid;
            Subgroups.CollectionChanged += Subgroups_CollectionChanged;
        }

        public GuidStructureItem(string name, Guid layer)
        {
            Name = name;
            GroupGuid = Guid.NewGuid();
            Parent = null;
            StartLayerGuid = layer;
            EndLayerGuid = layer;
            Subgroups.CollectionChanged += Subgroups_CollectionChanged;
        }

        public override int GetHashCode()
        {
            return GroupGuid.GetHashCode();
        }

        public GuidStructureItem CloneGroup()
        {
            GuidStructureItem item = new(Name, StartLayerGuid, EndLayerGuid, Array.Empty<GuidStructureItem>(), null)
            {
                GroupGuid = GroupGuid,
                IsExpanded = isExpanded,
                IsRenaming = isRenaming
            };

            if(Subgroups.Count > 0)
            {
                item.Subgroups = new ObservableCollection<GuidStructureItem>();
                for (int i = 0; i < Subgroups.Count; i++)
                {
                    item.Subgroups.Add(Subgroups[i].CloneGroup());
                    item.Subgroups[^1].Parent = item;
                }
            }

            return item;
        }

        public object Clone()
        {
            return CloneGroup();
        }

        private void Subgroups_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var args = new GroupChangedEventArgs(e.NewItems != null ? e.NewItems.Cast<GuidStructureItem>().ToList() : new List<GuidStructureItem>());
            GroupsChanged?.Invoke(this, args);
            Parent?.GroupsChanged?.Invoke(this, args);
        }
    }
}