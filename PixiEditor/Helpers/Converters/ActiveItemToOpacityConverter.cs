using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Layers.Utils;
using PixiEditor.ViewModels;

namespace PixiEditor.Helpers.Converters
{
    [ValueConversion(typeof(object), typeof(float))]
    public class ActiveItemToOpacityConverter : IValueConverter
    {

        public Layer lastLayer //TODO this (ConvertBack)
        public GuidStructureItem structureItem;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is Layer layer)
            {
                return layer.Opacity * 100;
            }
            else if(value is GuidStructureItem group)
            {
                return group.Opacity;
            }

            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Layer layer)
            {
                return layer.Opacity * 100;
            }
            else if (value is GuidStructureItem group)
            {
                return group.Opacity;
            }
        }
    }
}