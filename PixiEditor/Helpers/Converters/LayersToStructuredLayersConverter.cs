using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using PixiEditor.Models.Layers;

namespace PixiEditor.Helpers.Converters
{
    public class LayersToStructuredLayersConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is IEnumerable<Layer> layers && values[1] is LayerStructure structure)
            {
                return new StructuredLayerTree(layers, structure).RootDirectoryItems;
            }

            return Binding.DoNothing;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            //if (value is ObservableCollection<object> tree)
            //{
            //    List<Layer> layers = new ();
            //    LayerStructure structure = new ();
            //    foreach (var branchLayers in tree.Select(x => x.Children))
            //    {
            //        //layers.AddRange(branchLayers);
            //        //structure.Items.Add(new GuidStructureItem(new ObservableCollection<Guid>(branchLayers.Select(x => x.LayerGuid))));
            //    }

            //    return new object[] { layers, structure };
            //}

            throw new ArgumentException("Value is not a StructuredLayerTree");
        }
    }
}