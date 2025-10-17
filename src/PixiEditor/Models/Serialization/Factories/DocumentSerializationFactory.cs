using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.BrushEngine;
using PixiEditor.Parser;
using PixiEditor.ViewModels.Document;

namespace PixiEditor.Models.Serialization.Factories;

internal class DocumentSerializationFactory : SerializationFactory<byte[], IReadOnlyDocument>
{
    public override string DeserializationId { get; } = "PixiEditor.Document";
    public override byte[] Serialize(IReadOnlyDocument original)
    {
        var vm = new DocumentViewModel(original);
        return PixiParser.V5.Serialize(vm.ToSerializable());
    }

    public override bool TryDeserialize(object serialized, out IReadOnlyDocument original,
        (string serializerName, string serializerVersion) serializerData)
    {
        if (serialized is byte[] bytes)
        {
            var doc = PixiParser.V5.Deserialize(bytes).ToDocument();
            original = doc.AccessInternalReadOnlyDocument();
            return true;
        }

        original = null;
        return false;
    }
}
