using Avalonia.Media;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Text;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.Models.Controllers;
using PixiEditor.UI.Common.Localization;

namespace PixiEditor.Models.Serialization.Factories;

internal class TextSerializationFactory : VectorShapeSerializationFactory<TextVectorData>
{
    public override string DeserializationId { get; } = "PixiEditor.Text";

    protected override void AddSpecificData(ByteBuilder builder, TextVectorData original)
    {
        builder.AddInt(original.Text.Inlines.Count);
        foreach (var textInline in original.Text.Inlines)
        {
            builder.AddString(textInline.Text);
            builder.AddInt((int)textInline.Alignment);
            builder.AddInt((int)textInline.LineHeight);
            builder.AddInt((int)textInline.StrokeWidth);
            AddPaintable(textInline.StrokePaintable, builder);
            builder.AddBool(textInline.Fill);
            AddPaintable(textInline.FillPaintable, builder);
            builder.AddString(textInline.Font.Family.Name);
            builder.AddBool(textInline.Font.Family.FontUri?.IsFile ?? false);
            if (textInline.Font.Family.FontUri?.IsFile ?? false)
            {
                builder.AddInt(Storage.AddFromFilePath(textInline.Font.Family.FontUri.LocalPath));
            }

            builder.AddDouble(textInline.Font.Size);
            builder.AddInt((int)textInline.Font.Weight);
            builder.AddInt((int)textInline.Font.Slant);
            builder.AddInt((int)textInline.Font.Edging);
            builder.AddBool(textInline.Font.SubPixel);
        }

        builder.AddDouble(original.MaxWidth);
        builder.AddBool(original.Path != null);
        if (original.Path != null)
        {
            builder.AddString(original.Path.ToSvgPathData());
        }

        builder.AddVecD(original.PathOffset);
        builder.AddBool(original.AntiAlias);
        builder.AddVecD(original.Position);
    }

    protected override bool DeserializeVectorData(ByteExtractor extractor, Matrix3X3 matrix, Paintable strokePaintable,
        bool fill, Paintable fillPaintable,
        float strokeWidth, (string serializerName, string serializerVersion) serializerData,
        out TextVectorData original)
    {
        if (IsFilePreVersion(serializerData, new Version(2, 2, 0, 1)))
        {
            return DeserializeOld(extractor, matrix, strokePaintable, fill, fillPaintable, strokeWidth, serializerData,
                out original);
        }

        int inlinesCount = extractor.GetInt();
        List<TextInline> inlines = new List<TextInline>(inlinesCount);
        for (int i = 0; i < inlinesCount; i++)
        {
            TextInline inline = DeserializeInline(extractor, serializerData);
            inlines.Add(inline);
        }

        double maxWidth = extractor.GetDouble();
        bool hasPath = extractor.GetBool();
        VectorPath path = null;
        if (hasPath)
        {
            path = VectorPath.FromSvgPath(extractor.GetString());
        }

        VecD pathOffset = extractor.GetVecD();
        bool antiAlias = extractor.GetBool();
        VecD position = extractor.GetVecD();

        original = new TextVectorData(new RichText(inlines))
        {
            TransformationMatrix = matrix,
            Stroke = strokePaintable,
            Fill = fill,
            FillPaintable = fillPaintable,
            StrokeWidth = strokeWidth,
            MaxWidth = maxWidth,
            Path = path,
            MissingFontText = new LocalizedString("MISSING_FONT"),
            PathOffset = pathOffset,
            AntiAlias = antiAlias,
            Position = position
        };
        return true;
    }

    private TextInline DeserializeInline(ByteExtractor extractor, (string serializerName, string serializerVersion) serializerData)
    {
        string text = extractor.GetString();
        TextAlign alignment = (TextAlign)extractor.GetInt();
        double lineHeight = extractor.GetInt();
        double strokeWidth = extractor.GetInt();
        Paintable strokePaintable = TryGetPaintable(extractor, false, serializerData);
        bool fill = extractor.GetBool();
        Paintable fillPaintable = TryGetPaintable(extractor, false, serializerData);
        string fontFamily = extractor.GetString();
        bool isFontFromFile = extractor.GetBool();
        string fontPath = null;
        if (isFontFromFile && ResourceLocator != null)
        {
            fontPath = Path.Combine(ResourceLocator.GetFilePath(extractor.GetInt()));
        }

        double fontSize = extractor.GetDouble();
        FontStyleWeight fontWeight = (FontStyleWeight)extractor.GetInt();
        FontStyleSlant fontSlant = (FontStyleSlant)extractor.GetInt();
        FontEdging fontEdging = (FontEdging)extractor.GetInt();
        bool fontSubPixel = extractor.GetBool();

        FontFamilyName family =
            new FontFamilyName(fontFamily) { FontUri = isFontFromFile ? new Uri(fontPath, UriKind.Absolute) : null };
        FontData font = new FontData();
        font.Family = family;
        if (isFontFromFile)
        {
            FontLibrary.TryAddCustomFont(family);
        }

        font.Weight = fontWeight;
        font.Slant = fontSlant;
        font.Edging = fontEdging;
        font.SubPixel = fontSubPixel;
        font.Size = fontSize;

        TextInline inline = new TextInline(text, font)
        {
            Alignment = alignment,
            LineHeight = (float)lineHeight,
            StrokeWidth = (float)strokeWidth,
            StrokePaintable = strokePaintable,
            Fill = fill,
            FillPaintable = fillPaintable
        };

        return inline;
    }

    private bool DeserializeOld(ByteExtractor extractor, Matrix3X3 matrix, Paintable strokePaintable, bool fill,
        Paintable fillPaintable, float strokeWidth, (string serializerName, string serializerVersion) serializerData,
        out TextVectorData original)
    {
        string text = DeserializeStringCompatible(extractor, serializerData);

        VecD position = extractor.GetVecD();
        bool antiAlias = extractor.GetBool();
        string fontFamily = DeserializeStringCompatible(extractor, serializerData);
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
        bool hasPath = extractor.GetBool();
        VectorPath path = null;
        if (hasPath)
        {
            path = VectorPath.FromSvgPath(DeserializeStringCompatible(extractor, serializerData));
        }

        VecD pathOffset = VecD.Zero;
        if (!IsFilePreVersion(serializerData, new Version(2, 0, 0, 95)))
        {
            pathOffset = extractor.GetVecD();
        }

        FontFamilyName family =
            new FontFamilyName(fontFamily) { FontUri = isFontFromFile ? new Uri(fontPath, UriKind.Absolute) : null };
        FontData font = new FontData() { Family = family };

        if (isFontFromFile)
        {
            FontLibrary.TryAddCustomFont(family);
        }

        font.Weight = bold ? FontStyleWeight.Bold : FontStyleWeight.Normal;
        font.Slant = italic ? FontStyleSlant.Italic : FontStyleSlant.Upright;
        font.Edging = antiAlias ? FontEdging.AntiAlias : FontEdging.Alias;
        font.SubPixel = antiAlias;
        font.Size = fontSize;

        original = new TextVectorData(new RichText(text, font))
        {
            TransformationMatrix = matrix,
            Stroke = strokePaintable,
            Fill = fill,
            FillPaintable = fillPaintable,
            StrokeWidth = strokeWidth,
            Position = position,
            MaxWidth = maxWidth,
            Path = path,
            MissingFontText = new LocalizedString("MISSING_FONT"),
            AntiAlias = antiAlias,
            PathOffset = pathOffset
        };
        return true;
    }
}
