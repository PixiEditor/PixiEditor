using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using PixiEditor.Models.Enums;

namespace PixiEditor.Helpers.Converters;
internal class StructureMemberSelectionTypeToColorConverter : IValueConverter
{
    public SolidColorBrush NoneColor { get; set; }
    public SolidColorBrush SoftColor { get; set; }
    public SolidColorBrush HardColor { get; set; }
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value switch
        {
            StructureMemberSelectionType.Hard => HardColor,
            StructureMemberSelectionType.Soft => SoftColor,
            StructureMemberSelectionType.None => NoneColor,
            _ => NoneColor,
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
