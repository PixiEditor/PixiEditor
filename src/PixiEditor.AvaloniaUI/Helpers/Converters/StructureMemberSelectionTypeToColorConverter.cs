using System.Globalization;
using Avalonia.Media;
using PixiEditor.AvaloniaUI.Models.Layers;
using PixiEditor.UI.Common.Converters;

namespace PixiEditor.AvaloniaUI.Helpers.Converters;
internal class StructureMemberSelectionTypeToColorConverter : SingleInstanceConverter<StructureMemberSelectionTypeToColorConverter>
{
    // Can't use DynamicResource, because properties are not AvaloniaProperty
    public IBrush NoneColor { get; set; }
    public IBrush SoftColor { get; set; }
    public IBrush HardColor { get; set; }

    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value switch
        {
            StructureMemberSelectionType.Hard => HardColor,
            StructureMemberSelectionType.Soft => SoftColor,
            StructureMemberSelectionType.None => NoneColor,
            _ => NoneColor,
        };
    }
}
