using System.Globalization;

namespace PixiEditor.Helpers.Converters;

internal class FilePathToFileNameConverter : SingleInstanceConverter<FilePathToFileNameConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string path)
        {
            return System.IO.Path.GetFileName(path);
        }

        return value!;
    }
}
