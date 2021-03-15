using System.Collections.ObjectModel;

namespace PixiEditor.Models.Layers
{
    public class LayerStructureItem
    {
        public ObservableCollection<ILayerContainer> Children { get; set; } = new ObservableCollection<ILayerContainer>();

        public LayerStructureItem(ObservableCollection<ILayerContainer> children)
        {
            Children = children;
        }
    }
}