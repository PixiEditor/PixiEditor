using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.BrushEngine;
using PixiEditor.Parser;
using PixiEditor.ViewModels.Document;
using PixiEditor.ViewModels.Document.Nodes.Brushes;

namespace PixiEditor.Models.Serialization.Factories;

internal class BrushSerializationFactory : SerializationFactory<byte[], Brush>
{
    public override string DeserializationId { get; } = "PixiEditor.Brush";

    public override byte[] Serialize(Brush original)
    {
        ByteBuilder builder = new ByteBuilder();
        builder.AddString(original.Name);
        byte[] bytes = PixiParser.V5.Serialize(((DocumentViewModel)original.Document).ToSerializable());
        builder.AddInt(bytes.Length);
        builder.AddByteArray(bytes);

        return builder.Build();
    }

    public override bool TryDeserialize(object serialized, out Brush original,
        (string serializerName, string serializerVersion) serializerData)
    {
        if (serialized is byte[] bytes)
        {
            ByteExtractor extractor = new ByteExtractor(bytes);
            string name = extractor.GetString();
            int docLength = extractor.GetInt();
            byte[] docBytes = extractor.GetByteSpan(docLength).ToArray();
            var doc = PixiParser.V5.Deserialize(docBytes).ToDocument();
            original = new Brush(name, doc, "EMBEDDED", null)
            {
                IsDuplicable = false,
                IsReadOnly = true
            };

            return true;
        }

        original = null;
        return false;
    }
}
