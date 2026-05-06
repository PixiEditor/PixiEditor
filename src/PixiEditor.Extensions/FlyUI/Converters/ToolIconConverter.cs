using System.Globalization;
using Avalonia.Data.Converters;
using PixiEditor.Extensions.IO;

namespace PixiEditor.Extensions.FlyUI.Converters;

public class ToolIconConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values is not [string icon, IResourceStorage storage])
            return null;

        return PathToImgSourceConverter.GetImageFromPath(icon, storage);
    }
}
