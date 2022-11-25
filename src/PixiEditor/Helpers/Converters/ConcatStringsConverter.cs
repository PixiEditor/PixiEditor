using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixiEditor.Helpers.Converters;
internal class ConcatStringsConverter : SingleInstanceMultiValueConverter<ConcatStringsConverter>
{
    public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        string separator = parameter is string str ? str : "";
        string combined = "";
        bool first = true;
        foreach (var entry in values)
        {
            if (entry is not { })
                continue;
            string toConcat;
            if (entry is string entryString)
                toConcat = entryString;
            else
                toConcat = entry.ToString();
            if (toConcat != "")
            {
                if (!first)
                    combined += separator;
                first = false;
                combined += toConcat;
            }
        }
        return combined;
    }
}
