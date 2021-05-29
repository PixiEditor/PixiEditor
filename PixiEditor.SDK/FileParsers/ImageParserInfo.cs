using System;
using System.Windows.Media.Imaging;

namespace PixiEditor.SDK.FileParsers
{
    internal class ImageParserInfo : FileParserInfo<ImageParser, WriteableBitmap>
    {
        public ImageParserInfo(Type imageParserType)
            : base(imageParserType)
        {
        }
    }
}
