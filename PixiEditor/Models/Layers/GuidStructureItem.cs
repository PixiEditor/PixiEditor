using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using PixiEditor.Helpers;

namespace PixiEditor.Models.Layers
{
    [DebuggerDisplay("{Name} - {FolderGuid}")]
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

        private Guid? startLayerGuid = null;

        public Guid? StartLayerGuid
        {
            get => startLayerGuid;
            set
            {
                startLayerGuid = value;
                RaisePropertyChanged(nameof(StartLayerGuid));
            }
        }

        private Guid? endLayerGuid = null;

        public Guid? EndLayerGuid
        {
            get => endLayerGuid;
            set
            {
                endLayerGuid = value;
                RaisePropertyChanged(nameof(EndLayerGuid));
            }
        }

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

        public GuidStructureItem(
            string name,
            Guid startLayerGuid,
            Guid endLayerGuid,
            IEnumerable<GuidStructureItem> subfolders,
            GuidStructureItem parent)
        {
            Name = name;
            Subfolders = new ObservableCollection<GuidStructureItem>(subfolders);
            FolderGuid = Guid.NewGuid();
            Parent = parent;
            StartLayerGuid = startLayerGuid;
            EndLayerGuid = endLayerGuid;
        }

        public GuidStructureItem(string name, Guid layer)
        {
            Name = name;
            Subfolders = new ObservableCollection<GuidStructureItem>();
            FolderGuid = Guid.NewGuid();
            Parent = null;
            StartLayerGuid = layer;
            EndLayerGuid = layer;
        }
    }
}