using System;

namespace AvaloniaGif.Decoding
{
    public class GifFrame
    {
        public bool HasTransparency, IsInterlaced, IsLocalColorTableUsed;
        public byte TransparentColorIndex;
        public int LZWMinCodeSize, LocalColorTableSize;
        public long LZWStreamPosition;
        public TimeSpan FrameDelay;
        public FrameDisposal FrameDisposalMethod;
        public bool ShouldBackup;
        public GifRect Dimensions;
        public GifColor[] LocalColorTable;
    }
}