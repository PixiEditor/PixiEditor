using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace PixiEditor.Helpers.Converters
{
    class ViewboxInverseTransformConverter : MultiValueMarkupConverter
    {
        public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var transform = ((ContainerVisual)VisualTreeHelper.GetChild((DependencyObject)values[0], 0)).Transform;
            if (transform == null)
                return DependencyProperty.UnsetValue;
            return transform.Inverse;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
