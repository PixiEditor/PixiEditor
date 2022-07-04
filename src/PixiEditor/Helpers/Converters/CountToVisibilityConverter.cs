using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace PixiEditor.Helpers.Converters;

public class CountToVisibilityConverter : SingleInstanceConverter<CountToVisibilityConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int intVal)
        {
            return intVal == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        return Visibility.Visible;
    }
}