using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PixiEditor.Helpers.Converters;
internal class BoolOrToVisibilityConverter : SingleInstanceMultiValueConverter<BoolOrToVisibilityConverter>
{
    public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        bool boolean = values.Aggregate(false, (acc, cur) => acc |= ((cur as bool?) ?? false));
        return boolean ? Visibility.Visible : Visibility.Collapsed;
    }
}
