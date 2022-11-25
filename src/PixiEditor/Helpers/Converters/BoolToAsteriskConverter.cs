using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PixiEditor.Helpers.Converters;
internal class BoolToAsteriskConverter : SingleInstanceConverter<BoolToAsteriskConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not bool boolean)
            return DependencyProperty.UnsetValue;
        return boolean ? "" : "*";
    }
}
