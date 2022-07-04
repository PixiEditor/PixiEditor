using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixiEditor.Helpers.Converters;

public class IndexToAssociatedKeyConverter : SingleInstanceConverter<IndexToAssociatedKeyConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if(value is int index && index < 10)
        {
            if (index == 9) return 0;
            return (int?)index + 1;
        }

        return (int?)null;
    }
}