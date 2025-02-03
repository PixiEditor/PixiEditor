using System.Text;
using Drawie.Backend.Core.Text;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.Helpers;
using PixiEditor.Models.Controllers;

namespace PixiEditor.Models.IO.CustomDocumentFormats;

internal class FontDocumentBuilder : IDocumentBuilder
{
    public IReadOnlyCollection<string> Extensions { get; } = [".ttf", ".otf"];

    public void Build(DocumentViewModelBuilder builder, string path)
    {
        Font font = Font.FromFontFamily(new FontFamilyName(new Uri(path), Path.GetFileNameWithoutExtension(path)));
        font.Size = 12;

        List<char> glyphs = new();
        int lastGlyph = 0;
        for (int i = 0; i < char.MaxValue; i++)
        {
            if (font.ContainsGlyph(i) && font.MeasureText(((char)i).ToString()) > 0)
            {
                lastGlyph++;
                glyphs.Add((char)i);
                if (lastGlyph >= font.GlyphCount - 1)
                {
                    break;
                }
            }
        }

        int rows = (int)Math.Ceiling(Math.Sqrt(glyphs.Count));
        int cols = (int)Math.Ceiling((double)glyphs.Count / rows);

        StringBuilder sb = new();
        for (int i = 0; i < glyphs.Count; i++)
        {
            sb.Append(glyphs[i]);
            if (i % cols == cols - 1)
            {
                sb.Append('\n');
            }
        }

        TextVectorData textData = new() { Text = sb.ToString(), Font = font, StrokeWidth = 0, Spacing = 12 };
        RectD bounds = textData.GeometryAABB;

        const int padding = 1;

        FontLibrary.TryAddCustomFont(font.Family);

        textData.Position = new VecD(0, font.Size);

        builder.WithSize((int)Math.Ceiling(bounds.Width) + padding, (int)Math.Ceiling(bounds.Height) + padding)
            .WithGraph(graph =>
            {
                graph.WithNodeOfType<VectorLayerNode>(out int id)
                    .WithName(font.Family.Name)
                    .WithAdditionalData(new Dictionary<string, object>() { { "ShapeData", textData } });
                graph.WithOutputNode(id, "Output");
            });
    }
}
