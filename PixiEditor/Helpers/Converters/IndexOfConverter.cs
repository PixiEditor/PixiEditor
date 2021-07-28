using PixiEditor.Models.Controllers;
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
            BitmapManager bitmapManager = ViewModelMain.Current.BitmapManager;

            return value is Layer layer && bitmapManager.ActiveDocument != null
                   ? bitmapManager.ActiveDocument.Layers.IndexOf(layer)
                   : Binding.DoNothing;
        }
    }
}