using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PixiEditor.Helpers;
using PixiEditor.ViewModels;

namespace PixiEditor.Models.Layers
{
    public class LayerGroup : NotifyableObject
    {
        public Guid GroupGuid { get; init; }

        public GuidStructureItem StructureData { get; init; }

        public ObservableCollection<Layer> Layers { get; set; } = new ObservableCollection<Layer>();

        public ObservableCollection<LayerGroup> Subfolders { get; set; } = new ObservableCollection<LayerGroup>();

        public IEnumerable Items => BuildItems();

        private IEnumerable BuildItems()
        {
            List<object> obj = new(Layers.Reverse());
            foreach (var subfolder in Subfolders)
            {
                obj.Insert(subfolder.DisplayIndex - DisplayIndex, subfolder);
            }

            obj.Reverse();

            return obj;
        }

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

        private bool isExpanded = false;

        public bool IsExpanded
        {
            get => isExpanded;
            set
            {
                isExpanded = value;
                UpdateIsExpandedInDocument(value);
                RaisePropertyChanged(nameof(IsExpanded));
            }
        }

        private int displayIndex;

        public int DisplayIndex
        {
            get => displayIndex;
            set
            {
                displayIndex = value;
                RaisePropertyChanged(nameof(DisplayIndex));
            }
        }

        private int topIndex;

        public int TopIndex
        {
            get => topIndex;
            set
            {
                topIndex = value;
                RaisePropertyChanged(nameof(TopIndex));
            }
        }

        private void UpdateIsExpandedInDocument(bool value)
        {
            var folder = ViewModelMain.Current.BitmapManager.ActiveDocument.LayerStructure.GetGroupByGuid(GroupGuid);
            if (folder != null)
            {
                folder.IsExpanded = value;
            }
        }

        public LayerGroup(IEnumerable<Layer> layers, IEnumerable<LayerGroup> subfolders, string name,
            int displayIndex, int topIndex, GuidStructureItem structureData)
            : this(layers, subfolders, name, Guid.NewGuid(), displayIndex, topIndex, structureData)
        {
        }

        public LayerGroup(IEnumerable<Layer> layers, IEnumerable<LayerGroup> subfolders, string name,
            Guid guid, int displayIndex, int topIndex, GuidStructureItem structureData)
        {
            Layers = new ObservableCollection<Layer>(layers);
            Subfolders = new ObservableCollection<LayerGroup>(subfolders);
            Name = name;
            GroupGuid = guid;
            DisplayIndex = displayIndex;
            TopIndex = topIndex;
            StructureData = structureData;
        }
    }
}