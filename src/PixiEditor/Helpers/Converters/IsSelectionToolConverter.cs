using System.Globalization;
using PixiEditor.ViewModels.Tools.Tools;

namespace PixiEditor.Helpers.Converters;

internal class IsSelectionToolConverter : SingleInstanceConverter<IsSelectionToolConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is SelectToolViewModel or LassoToolViewModel or MagicWandToolViewModel;
}
