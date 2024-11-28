using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Avalonia.Controls;
using Drawie.Backend.Core.ColorsImpl;

namespace PixiEditor.Helpers.Converters;

internal class PaletteItemsHeightConverter : SingleInstanceConverter<PaletteItemsHeightConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ItemCollection items)
        {
            double itemSize = 21;
            return items.Count * itemSize;
        }

        return value;
    }
}
