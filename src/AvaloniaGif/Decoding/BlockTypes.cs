namespace AvaloniaGif.Decoding
{
    internal enum BlockTypes
    {
        EMPTY = 0,
        EXTENSION = 0x21,
        IMAGE_DESCRIPTOR = 0x2C,
        TRAILER = 0x3B,
    }
}