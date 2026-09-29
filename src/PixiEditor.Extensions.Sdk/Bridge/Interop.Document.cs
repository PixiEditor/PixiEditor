using PixiEditor.Extensions.CommonApi.Documents;
using PixiEditor.Extensions.CommonApi.Palettes;
using PixiEditor.Extensions.Sdk.Api.Documents;
using PixiEditor.Extensions.Sdk.Utilities;
using ProtoBuf;

namespace PixiEditor.Extensions.Sdk.Bridge;

internal static partial class Interop
{
    public static IDocument GetActiveDocument()
    {
        string document = Native.get_active_document();
        if (document == null || !Guid.TryParse(document, out Guid id))
            return null;

        return new Document(id);
    }

    public static IDocument? ImportFile(string path, bool associatePath)
    {
        string document = Native.import_file(path, associatePath);
        if (document == null || !Guid.TryParse(document, out Guid id))
            return null;

        return new Document(id);
    }
    public static IDocument? ImportDocument(byte[] data)
    {
        IntPtr ptr = InteropUtility.ByteArrayToIntPtr(data);
        string document = Native.import_document(ptr, data.Length);
        InteropUtility.FreeIntPtr(ptr);
        if (document == null || !Guid.TryParse(document, out Guid id))
            return null;

        return new Document(id);
    }

    public static PaletteColor[] GetDocumentPalette(Guid documentId)
    {
        IntPtr palettePtr = Native.get_document_palette(documentId.ToString());
        byte[] colors = InteropUtility.PrefixedIntPtrToByteArray(palettePtr);
        using MemoryStream stream = new(colors);
        var palette = Serializer.Deserialize<PaletteColor[]>(stream);

        return palette;
    }

    public static void SetDocumentPalette(Guid documentId, PaletteColor[] palette)
    {
        using MemoryStream stream = new();
        Serializer.Serialize(stream, palette);
        byte[] data = stream.ToArray();
        IntPtr palettePtr = InteropUtility.ByteArrayToIntPtr(data);
        Native.set_document_palette(documentId.ToString(), palettePtr, data.Length);

        InteropUtility.FreeIntPtr(palettePtr);
    }
}
