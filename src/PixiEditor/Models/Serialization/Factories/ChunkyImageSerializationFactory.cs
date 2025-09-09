using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;
using PixiEditor.Extensions.CommonApi.Utilities;

namespace PixiEditor.Models.Serialization.Factories;

public class ChunkyImageSerializationFactory : SerializationFactory<byte[], ChunkyImage>
{
    public override byte[] Serialize(ChunkyImage original)
    {
        var chunks = original.CloneAllCommitedNonEmptyChunks();
        ByteBuilder builder = new();
        builder.AddVecD(original.CommittedSize);

        SurfaceSerializationFactory surfaceFactory = new();
        surfaceFactory.Config = Config;

        builder.AddInt(chunks.Count);
        foreach (var chunk in chunks)
        {
            builder.AddVecD(chunk.Key);
            byte[] serialized = surfaceFactory.Serialize(chunk.Value);
            builder.AddInt(serialized.Length);
            builder.AddByteArray(serialized);
        }

        return builder.Build();
    }

    public override bool TryDeserialize(object serialized, out ChunkyImage original,
        (string serializerName, string serializerVersion) serializerData)
    {
        SurfaceSerializationFactory surfaceFactory = new();
        surfaceFactory.Config = Config;
        if (IsFilePreVersion(serializerData, new Version(2, 0, 1, 14)) || serializerData == default)
        {
            if (serialized is byte[] imgBytes)
            {
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

        if (serialized is not byte[] bytes)
        {
            original = null;
            return false;
        }

        ByteExtractor byteExtractor = new(bytes);
        VecD size = byteExtractor.GetVecD();
        original = new ChunkyImage((VecI)size, Config.ProcessingColorSpace);
        int chunkCount = byteExtractor.GetInt();

        for (int i = 0; i < chunkCount; i++)
        {
            VecD chunkPos = byteExtractor.GetVecD();
            int chunkDataLength = byteExtractor.GetInt();
            Span<byte> chunkData = byteExtractor.GetByteSpan(chunkDataLength);
            if (!surfaceFactory.TryDeserialize(chunkData, out Surface chunkSurface, serializerData))
            {
                original.Dispose();
                original = null;
                return false;
            }

            RectD chunkRect = new RectD(chunkPos * chunkSurface.Size.X, chunkSurface.Size);
            original.EnqueueDrawImage((VecI)chunkRect.TopLeft, chunkSurface);
            chunkSurface.Dispose();
        }

        original.CommitChanges();
        return true;
    }

    public override string DeserializationId { get; } = "PixiEditor.ChunkyImage";
}
