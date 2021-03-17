using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PixiEditor.Helpers;

namespace PixiEditor.Models.Layers
{
    public class LayerFolder : NotifyableObject
    {
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

        public LayerFolder(IEnumerable<Layer> layers, IEnumerable<LayerFolder> subfolders, string name)
        {
            Layers = new ObservableCollection<Layer>(layers);
            Subfolders = new ObservableCollection<LayerFolder>(subfolders);
            Name = name;
        }
    }
}