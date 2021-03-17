using System.Collections.ObjectModel;

namespace PixiEditor.Models.Layers
{
    public class LayerStructure
    {
        public ObservableCollection<GuidStructureItem> Folders { get; set; }

        public LayerStructure(ObservableCollection<GuidStructureItem> items)
        {
            Folders = items;
        }

        public LayerStructure()
        {
            Folders = new ObservableCollection<GuidStructureItem>();
        }
    }
}