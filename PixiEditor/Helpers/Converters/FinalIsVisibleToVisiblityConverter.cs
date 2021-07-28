using PixiEditor.Models.Controllers;
using PixiEditor.Models.Layers;
using PixiEditor.ViewModels;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace PixiEditor.Helpers.Converters
{
    public class FinalIsVisibleToVisiblityConverter
        : SingleInstanceMultiValueConverter<FinalIsVisibleToVisiblityConverter>
    {
        public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            BitmapManager bitmapManager = ViewModelMain.Current?.BitmapManager;

            return
                (values[0] is not Layer layer ||
                bitmapManager.ActiveDocument is null ||
                bitmapManager.ActiveDocument.GetFinalLayerIsVisible(layer))
                    ? Visibility.Visible
                    : (object)Visibility.Collapsed;
        }
    }
}