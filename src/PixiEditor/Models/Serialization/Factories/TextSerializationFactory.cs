using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Text;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.IO;

namespace PixiEditor.Models.Serialization.Factories;

internal class TextSerializationFactory : VectorShapeSerializationFactory<TextVectorData>
{
    public override string DeserializationId { get; } = "PixiEditor.Text";

    protected override void AddSpecificData(ByteBuilder builder, TextVectorData original)
    {
        builder.AddString(original.Text);
        builder.AddVecD(original.Position);
        builder.AddBool(original.AntiAlias);
        builder.AddString(original.Font.Family.Name);
        builder.AddBool(original.Font.Family.FontUri?.IsFile ?? false);
        if (original.Font.Family.FontUri?.IsFile ?? false)
        {
            builder.AddInt(Storage.AddFromFilePath(original.Font.Family.FontUri.LocalPath));
        }

        builder.AddDouble(original.Font.Size);
        builder.AddBool(original.Font.Bold);
        builder.AddBool(original.Font.Italic);

        builder.AddDouble(original.MaxWidth);
        builder.AddDouble(original.Spacing ?? original.Font.Size);
        builder.AddBool(original.Path != null);
        if (original.Path != null)
        {
            builder.AddString(original.Path.ToSvgPathData());
        }
    }

    protected override bool DeserializeVectorData(ByteExtractor extractor, Matrix3X3 matrix, Paintable strokePaintable,
        bool fill, Paintable fillPaintable,
        float strokeWidth, (string serializerName, string serializerVersion) serializerData,
        out TextVectorData original)
    {
        string text = extractor.GetString();
        VecD position = extractor.GetVecD();
        bool antiAlias = extractor.GetBool();
        string fontFamily = extractor.GetString();
        bool isFontFromFile = extractor.GetBool();
        string fontPath = null;
        if (isFontFromFile && ResourceLocator != null)
        {
            fontPath = Path.Combine(ResourceLocator.GetFilePath(extractor.GetInt()));
        }

        double fontSize = extractor.GetDouble();
        bool bold = extractor.GetBool();
        bool italic = extractor.GetBool();

        double maxWidth = extractor.GetDouble();
        double spacing = extractor.GetDouble();
        bool hasPath = extractor.GetBool();
        VectorPath path = null;
        if (hasPath)
        {
            path = VectorPath.FromSvgPath(extractor.GetString());
        }

        FontFamilyName family =
            new FontFamilyName(fontFamily) { FontUri = isFontFromFile ? new Uri(fontPath, UriKind.Absolute) : null };
        Font font = Font.FromFontFamily(family);
        FontFamilyName? missingFamily = null;

        if (font == null)
        {
            font = Font.CreateDefault();
            missingFamily = family;
        }
        else if (isFontFromFile)
        {
            FontLibrary.TryAddCustomFont(family);
        }


        font.Bold = bold;
        font.Italic = italic;
        font.Edging = antiAlias ? FontEdging.AntiAlias : FontEdging.Alias;
        font.SubPixel = antiAlias;
        font.Size = fontSize;

        original = new TextVectorData(text)
        {
            TransformationMatrix = matrix,
            Stroke = strokePaintable,
            Fill = fill,
            FillPaintable = fillPaintable,
            StrokeWidth = strokeWidth,
            Position = position,
            Font = font,
            MaxWidth = maxWidth,
            Spacing = spacing,
            Path = path,
            MissingFontFamily = missingFamily,
            MissingFontText = new LocalizedString("MISSING_FONT"),
            AntiAlias = antiAlias
        };

        return true;
    }
}
