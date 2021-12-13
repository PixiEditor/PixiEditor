using PixiEditor.Models.DataHolders;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PixiEditor.Helpers.Converters
{
    class DockingManagerActiveContentConverter : IValueConverter
    {
        private Document cachedDocument = null;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return DependencyProperty.UnsetValue;
            if (value is Document document)
            {
                cachedDocument = document;
                return document;
            }
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Document document)
                return document;
            if (value != null && cachedDocument != null && !cachedDocument.Disposed)
                return cachedDocument;
            cachedDocument = null;
            return DependencyProperty.UnsetValue;
        }
    }
}
