using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;

namespace PixiEditor.Models.Serialization.Factories;

public class ChunkyImageSerializationFactory : SerializationFactory<byte[], ChunkyImage>
{
    private static SurfaceSerializationFactory surfaceFactory = new();

    public override byte[] Serialize(ChunkyImage original)
    {
        var encoder = Config.Encoder;
        surfaceFactory.Config = Config;

        using Surface surface = new Surface(original.LatestSize);
        original.DrawMostUpToDateRegionOn(
            new RectI(0, 0, original.LatestSize.X,
                original.LatestSize.Y), ChunkResolution.Full, surface.DrawingSurface, new VecI(0, 0), new Paint());

        return surfaceFactory.Serialize(surface);
    }

    public override bool TryDeserialize(object serialized, out ChunkyImage original,
        (string serializerName, string serializerVersion) serializerData)
    {
        if (serialized is byte[] imgBytes)
        {
            surfaceFactory.Config = Config;
            if (!surfaceFactory.TryDeserialize(imgBytes, out Surface surface, serializerData))
            {
                original = null;
                return false;
            }

            original = new ChunkyImage(surface.Size, Config.ProcessingColorSpace);
            original.EnqueueDrawImage(VecI.Zero, surface);
            original.CommitChanges();
            surface.Dispose();
            return true;
        }

        original = null;
        return false;
    }

    public override string DeserializationId { get; } = "PixiEditor.ChunkyImage";
}
