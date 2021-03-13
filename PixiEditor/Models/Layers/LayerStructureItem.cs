using System.Collections.ObjectModel;

namespace PixiEditor.Models.Layers
{
    public class LayerStructureItem
    {
        public ObservableCollection<Layer> Children { get; set; } = new ObservableCollection<Layer>();

        public LayerStructureItem(ObservableCollection<Layer> children)
        {
            Children = children;
        }
    }
}