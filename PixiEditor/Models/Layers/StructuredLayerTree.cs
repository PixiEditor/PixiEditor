using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PixiEditor.Models.Layers
{
    public class StructuredLayerTree
    {
        public ObservableCollection<LayerStructureItem> Items { get; set; } = new ObservableCollection<LayerStructureItem>();

        public StructuredLayerTree(IEnumerable<Layer> layers, LayerStructure structure)
        {
            if (structure == null || structure.Items.Count == 0)
            {
                foreach (var layer in layers)
                {
                    Items.Add(new LayerStructureItem(new ObservableCollection<Layer> { layer }));
                }

                return;
            }

            for (int i = 0; i < structure.Items.Count; i++)
            {
                var itemChildren = new ObservableCollection<Layer>();
                foreach (var guid in structure.Items[i].Children)
                {
                    itemChildren.Add(layers.First(x => x.LayerGuid == guid));
                }

                Items.Add(new LayerStructureItem(itemChildren));
            }
        }

        public StructuredLayerTree()
        {
            
        }
    }
}