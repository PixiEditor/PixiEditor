using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using PixiEditor.Helpers;

namespace PixiEditor.Models.Layers
{
    [DebuggerDisplay("{Name} - {GroupGuid}")]
    public class GuidStructureItem : NotifyableObject
    {
        public event EventHandler GroupsChanged;

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
            set
            {
                SetProperty(ref isRenaming, value);
            }
        }

        public GuidStructureItem(
            string name,
            Guid startLayerGuid,
            Guid endLayerGuid,
            IEnumerable<GuidStructureItem> subfolders,
            GuidStructureItem parent)
        {
            Name = name;
            Subgroups = new ObservableCollection<GuidStructureItem>(subfolders);
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

        private void Subgroups_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            GroupsChanged?.Invoke(this, EventArgs.Empty);
            Parent?.GroupsChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}