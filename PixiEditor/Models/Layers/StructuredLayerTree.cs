using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PixiEditor.Helpers.Extensions;

namespace PixiEditor.Models.Layers
{
    public class StructuredLayerTree
    {
        public ObservableCollection<LayerStructureItem> Items { get; set; } = new ObservableCollection<LayerStructureItem>();

        public StructuredLayerTree(IEnumerable<ILayerContainer> layers, LayerStructure structure)
        {
            if (structure == null || structure.Items.Count == 0)
            {
                foreach (var layer in layers)
                {
                    var collection = new ObservableCollection<ILayerContainer>();
                    collection.AddRange(layer.GetLayers());
                    Items.Add(new LayerStructureItem(collection));
                }

                return;
            }

            for (int i = 0; i < structure.Items.Count; i++)
            {
                var itemChildren = new ObservableCollection<ILayerContainer>();
                foreach (var guid in structure.Items[i].Children)
                {
                    itemChildren.Add(layers.First(x => x.GetLayers().First(y => y.LayerGuid == guid).LayerGuid == guid));
                }

                Items.Add(new LayerStructureItem(itemChildren));
            }
        }

        public StructuredLayerTree()
        {
        }
    }
}