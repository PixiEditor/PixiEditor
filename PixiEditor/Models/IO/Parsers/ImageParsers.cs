using PixiEditor.SDK.FileParsers;
using System.Windows.Media.Imaging;

#pragma warning disable SA1402 // File may only contain a single type

namespace PixiEditor.Models.IO.Parsers
{
    [FileParser(".png")]
    class PngParser : ImageParser
    {
        public override bool UseBigEndian => false;

        public override WriteableBitmap Parse()
        {
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = Stream;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();

            return BitmapFactory.ConvertToPbgra32Format(bitmap);
        }

        public override void Save(WriteableBitmap value)
        {
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(value));
            encoder.Save(Stream);
        }
    }

    [FileParser(".jpg", ".jpeg")]
    class JpegParser : ImageParser
    {
        public override bool UseBigEndian => false;

        public override WriteableBitmap Parse()
        {
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = Stream;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();

            return BitmapFactory.ConvertToPbgra32Format(bitmap);
        }

        public override void Save(WriteableBitmap value)
        {
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(value));
            encoder.Save(Stream);
        }
    }
}
