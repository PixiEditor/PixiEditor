using PixiEditor.ChangeableDocument.Changeables;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.Extensions.CommonApi.Utilities;
using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.BrushEngine;
using PixiEditor.Models.IO;
using PixiEditor.Parser;
using PixiEditor.ViewModels.Document;

namespace PixiEditor.Models.Serialization.Factories;

internal class DocumentSerializationFactory : SerializationFactory<byte[], DocumentReference>
{
    public override string DeserializationId { get; } = "PixiEditor.Document";

    public override byte[] Serialize(DocumentReference original)
    {
        var vm = new DocumentViewModel(original.DocumentInstance);
        string? originalFilePath = original.OriginalFilePath;

        int handle = Storage.AddFromBytes(original.ReferenceId.ToString(), PixiParser.V5.Serialize(vm.ToSerializable()));
        ByteWriter writer = new();
        writer.WriteString(originalFilePath ?? string.Empty);
        writer.WriteString(original.ReferenceId.ToString());
        writer.WriteInt(handle);
        return writer.ToArray();
    }

    public override bool TryDeserialize(object serialized, out DocumentReference original,
        (string serializerName, string serializerVersion) serializerData)
    {
        if (serialized is byte[] bytes)
        {
            ByteReader reader = new(bytes);
            string originalFilePath = reader.ReadString();
            string referenceId = reader.ReadString();
            Guid refIdGuid = Guid.Parse(referenceId);
            int handle = reader.ReadInt();

            if (ResourceLocator.ContainsInstance(handle))
            {
                var existing = ResourceLocator.GetInstance<DocumentViewModel>(handle);
                original = new DocumentReference(originalFilePath, refIdGuid, existing.AccessInternalReadOnlyDocument().Clone());
                return true;
            }

            if (File.Exists(originalFilePath))
            {
                var doc = Importer.ImportDocument(originalFilePath);
                ResourceLocator.RegisterInstance(handle, doc);
                original = new DocumentReference(originalFilePath, refIdGuid, doc.AccessInternalReadOnlyDocument().Clone());
                return true;
            }

            var data = ResourceLocator.TryGetInstanceOrLoad(handle, toLoad =>
            {
                return PixiParser.V5.Deserialize(toLoad).ToDocument();
            });

            if (data == null)
            {
                original = null;
                return false;
            }

            original = new DocumentReference(originalFilePath == string.Empty ? null : originalFilePath, refIdGuid, data.AccessInternalReadOnlyDocument());
            return true;
        }

        original = null;
        return false;
    }
}
