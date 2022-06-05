using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixiEditor.Helpers.Converters
{
    public class IsCollapsedToRowSpanConverter : SingleInstanceConverter<IsCollapsedToRowSpanConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isCollapsed = (bool)value;

            return isCollapsed ? 2 : 0;
        }
    }
}
