using System.Collections.ObjectModel;

namespace PixiEditor.Models.Layers
{
    public class LayerStructure
    {
        public ObservableCollection<GuidStructureItem> Items { get; set; }

        public LayerStructure(ObservableCollection<GuidStructureItem> items)
        {
            Items = items;
        }

        public LayerStructure()
        {
            Items = new ObservableCollection<GuidStructureItem>();
        }
    }
}