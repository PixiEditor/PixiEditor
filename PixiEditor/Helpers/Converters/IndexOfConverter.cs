using PixiEditor.Models.Layers;
using PixiEditor.ViewModels;
using System;
using System.Globalization;
using System.Windows.Data;

namespace PixiEditor.Helpers.Converters
{
    public class IndexOfConverter
        : SingleInstanceConverter<IndexOfConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Layer layer && ViewModelMain.Current.BitmapManager.ActiveDocument != null)
            {
                int index = ViewModelMain.Current.BitmapManager.ActiveDocument.Layers.IndexOf(layer);
                return index;
            }

            return value is Layer layer && bitmapManager.ActiveDocument != null
                   ? bitmapManager.ActiveDocument.Layers.IndexOf(layer)
                   : Binding.DoNothing;
        }
    }
}
