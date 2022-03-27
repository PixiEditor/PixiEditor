using ChunkyImageLib.DataHolders;
using System;
using System.Globalization;
using System.Windows.Data;

namespace PixiEditorPrototype.Converters
{
    internal class IndexToChunkResolutionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not int res)
                return ChunkResolution.Full;
            return res switch
            {
                0 => ChunkResolution.Full,
                1 => ChunkResolution.Half,
                2 => ChunkResolution.Quarter,
                3 => ChunkResolution.Eighth,
                _ => ChunkResolution.Full
            };
        }
    }
}
