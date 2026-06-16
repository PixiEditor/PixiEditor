using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using PixiEditor.AnimationRenderer.Core;

namespace PixiEditor.AnimationRenderer.FFmpeg;

public static class PipeUtil
{
    static readonly byte[] iend = new byte[] { 0x49, 0x45, 0x4E, 0x44, 0xAE, 0x42, 0x60, 0x82 };

    public static List<Bitmap> ReadFramesFromPipe(MemoryStream stream)
    {
        List<Bitmap> frames = new();
        byte[] data = stream.ToArray();

        int start = 0;
        for (int i = 0; i <= data.Length - iend.Length; i++)
        {
            if (data.AsSpan(i, iend.Length).SequenceEqual(iend))
            {
                int end = i + iend.Length;
                frames.Add(Bitmap.Decode(data[start..end]));
                start = end;
                i = end - 1;
            }
        }

        if (start < data.Length)
        {
            frames.Add(Bitmap.Decode(data[start..]));
        }

        return frames;
    }

}
