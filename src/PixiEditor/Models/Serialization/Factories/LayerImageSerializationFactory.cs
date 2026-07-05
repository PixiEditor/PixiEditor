using ChunkyImageLib;
using Drawie.Backend.Core;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables;
using PixiEditor.Extensions.CommonApi.Utilities;

namespace PixiEditor.Models.Serialization.Factories;

public class LayerImageSerializationFactory : SerializationFactory<byte[], LayerImage>
{
    public override byte[] Serialize(LayerImage original)
    {
        ChunkyImageSerializationFactory chunkyImageSerializationFactory = new ChunkyImageSerializationFactory();
        chunkyImageSerializationFactory.Config = Config;
        ByteBuilder builder = new();
        var bytes = chunkyImageSerializationFactory.Serialize(original.Main);
        builder.AddInt(bytes.Length);
        builder.AddByteArray(bytes);

        builder.AddInt(original.Additional?.Count ?? 0);
        if (original.Additional is not null)
        {
            foreach (var additional in original.Additional)
            {
                var additionalBytes = chunkyImageSerializationFactory.Serialize(additional);
                builder.AddInt(additionalBytes.Length);
                builder.AddByteArray(additionalBytes);
            }
        }

        return builder.Build();
    }

    public override bool TryDeserialize(object serialized, out LayerImage original,
        (string serializerName, string serializerVersion) serializerData)
    {
        if (serialized is not byte[] bytes)
        {
            original = null!;
            return false;
        }

        ChunkyImageSerializationFactory chunkyImageSerializationFactory = new ChunkyImageSerializationFactory();
        chunkyImageSerializationFactory.Config = Config;
        ByteReader reader = new(bytes);

        int mainLength = reader.ReadInt();
        var mainBytes = reader.ReadBytes(mainLength);
        var mainImage = chunkyImageSerializationFactory.Deserialize(mainBytes, serializerData) as ChunkyImage;
        if (mainImage is null)
        {
            original = null!;
            return false;
        }

        int additionalCount = reader.ReadInt();
        List<ChunkyImage>? additionalImages = null;
        if (additionalCount > 0)
        {
            additionalImages = new List<ChunkyImage>();
            for (int i = 0; i < additionalCount; i++)
            {
                int additionalLength = reader.ReadInt();
                var additionalBytes = reader.ReadBytes(additionalLength);
                var additionalImage = chunkyImageSerializationFactory.Deserialize(additionalBytes, serializerData) as ChunkyImage;
                if (additionalImage is null)
                {
                    original = null!;
                    return false;
                }

                additionalImages.Add(additionalImage);
            }
        }

        original = new LayerImage(mainImage) { Additional = additionalImages };
        return true;
    }

    public override string DeserializationId { get; } = "PixiEditor.LayerImage";
}
