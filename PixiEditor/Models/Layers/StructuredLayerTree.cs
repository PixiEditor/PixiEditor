using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PixiEditor.Helpers.Extensions;

namespace PixiEditor.Models.Layers
{
    public class StructuredLayerTree
    {
        public ObservableCollection<LayerStructureItem> Items { get; set; }

        public StructuredLayerTree(IEnumerable<Layer> layers, LayerStructure structure)
        {
            Items = new ();
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
    }
}