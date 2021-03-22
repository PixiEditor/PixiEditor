using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PixiEditor.Helpers;
using PixiEditor.ViewModels;

namespace PixiEditor.Models.Layers
{
    public class LayerFolder : NotifyableObject
    {
        public Guid FolderGuid { get; init; }

        public ObservableCollection<Layer> Layers { get; set; } = new ObservableCollection<Layer>();

        public ObservableCollection<LayerFolder> Subfolders { get; set; } = new ObservableCollection<LayerFolder>();

        public IEnumerable Items => Subfolders?.Cast<object>().Concat(Layers);

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
            var folder = ViewModelMain.Current.BitmapManager.ActiveDocument.LayerStructure.GetFolderByGuid(FolderGuid);
            if (folder != null)
            {
                folder.IsExpanded = value;
            }
        }

        public LayerFolder(IEnumerable<Layer> layers, IEnumerable<LayerFolder> subfolders, string name, int displayIndex, int topIndex)
            : this(layers, subfolders, name, Guid.NewGuid(), displayIndex, topIndex) { }

        public LayerFolder(IEnumerable<Layer> layers, IEnumerable<LayerFolder> subfolders, string name, Guid guid, int displayIndex, int topIndex)
        {
            Layers = new ObservableCollection<Layer>(layers);
            Subfolders = new ObservableCollection<LayerFolder>(subfolders);
            Name = name;
            FolderGuid = guid;
            DisplayIndex = displayIndex;
            TopIndex = topIndex;
        }
    }
}