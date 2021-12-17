using PixiEditor.Helpers;
using PixiEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PixiEditor.Models.Layers
{
    public class LayerGroup : NotifyableObject, IHasGuid
    {
        public Guid GuidValue { get; init; }

        private GuidStructureItem structureData;
        public GuidStructureItem StructureData
        {
            get => structureData;
            set => SetProperty(ref structureData, value);
        }

        private ObservableCollection<Layer> Layers { get; set; } = new ObservableCollection<Layer>();

        private ObservableCollection<LayerGroup> Subfolders { get; set; } = new ObservableCollection<LayerGroup>();

        private ObservableCollection<IHasGuid> items = null;
        public ObservableCollection<IHasGuid> Items => items ??= BuildItems();

        private ObservableCollection<IHasGuid> BuildItems()
        {
            List<IHasGuid> obj = new(Layers.Reverse());
            foreach (var subfolder in Subfolders)
            {
                obj.Insert(Math.Clamp(subfolder.DisplayIndex - DisplayIndex, 0, obj.Count), subfolder);
            }

            obj.Reverse();

            return new ObservableCollection<IHasGuid>(obj);
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

        private bool isRenaming;

        public bool IsRenaming
        {
            get => isRenaming;
            set
            {
                SetProperty(ref isRenaming, value);
            }
        }

        private void UpdateIsExpandedInDocument(bool value)
        {
            var folder = ViewModelMain.Current.BitmapManager.ActiveDocument.LayerStructure.GetGroupByGuid(GuidValue);
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
            GuidValue = guid;
            DisplayIndex = displayIndex;
            TopIndex = topIndex;
            StructureData = structureData;
        }
    }
}
