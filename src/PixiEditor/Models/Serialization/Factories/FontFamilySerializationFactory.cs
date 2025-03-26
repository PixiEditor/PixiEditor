using Drawie.Backend.Core.Text;
using PixiEditor.Models.Controllers;

namespace PixiEditor.Models.Serialization.Factories;

public class FontFamilySerializationFactory : SerializationFactory<byte[], FontFamilyName>
{
    public override string DeserializationId { get; } = "PixiEditor.FontFamilyName";

    public override byte[] Serialize(FontFamilyName original)
    {
        ByteBuilder builder = new ByteBuilder();

        if (original.Name == null)
        {
            original = new FontFamilyName(FontLibrary.DefaultFontFamily.Name) { FontUri = original.FontUri };
        }

        builder.AddString(original.Name);
        builder.AddBool(original.FontUri?.IsFile ?? false);
        if (original.FontUri?.IsFile ?? false)
        {
            builder.AddInt(Storage.AddFromFilePath(original.FontUri.LocalPath));
        }

        return builder.Build();
    }

    public override bool TryDeserialize(object serialized, out FontFamilyName original,
        (string serializerName, string serializerVersion) serializerData)
    {
        if (serialized is not byte[] bytes)
        {
            original = default;
            return false;
        }

        ByteExtractor extractor = new ByteExtractor(bytes);
        string fontFamily = extractor.GetString();
        bool isFontFromFile = extractor.GetBool();
        string fontPath = null;
        if (isFontFromFile && ResourceLocator != null)
        {
            fontPath = Path.Combine(ResourceLocator.GetFilePath(extractor.GetInt()));
        }

        FontFamilyName family =
            new FontFamilyName(fontFamily) { FontUri = isFontFromFile ? new Uri(fontPath, UriKind.Absolute) : null };

        if (isFontFromFile)
        {
            FontLibrary.TryAddCustomFont(family);
        }

        original = family;

        return true;
    }
}
