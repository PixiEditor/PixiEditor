using System.Globalization;
using ChunkyImageLib.Operations;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Text;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.ChangeableDocument.ChangeInfos.Structure;
using PixiEditor.ChangeableDocument.ChangeInfos.Vectors;
using PixiEditor.ChangeableDocument.Changes.NodeGraph;

namespace PixiEditor.ChangeableDocument.Changes.Text;

internal class ExtractSelectedText_Change : Change
{
    private readonly Guid memberId;
    private Guid[] newLayerIds;
    private bool extractEachCharacter = false;
    private int selectionStart;
    private int selectionEnd;
    private RichText? originalText = null;
    private List<(RichText text, VecD offset)>? subdivisions;
    private Dictionary<Guid, VecD> originalPositions = new Dictionary<Guid, VecD>();

    [GenerateMakeChangeAction]
    public ExtractSelectedText_Change(
        Guid memberId,
        int selectionStart,
        int selectionEnd,
        bool extractEachCharacter)
    {
        this.memberId = memberId;
        this.selectionStart = selectionStart;
        this.selectionEnd = selectionEnd;
        this.extractEachCharacter = extractEachCharacter;
    }

    public override bool InitializeAndValidate(Document target)
    {
        var node = target.FindNodeOrThrow<VectorLayerNode>(memberId);

        if (node.EmbeddedShapeData is not TextVectorData textData)
        {
            return false;
        }

        int minStart = Math.Min(selectionStart, selectionEnd);
        int maxEnd = Math.Max(selectionStart, selectionEnd);

        selectionStart = minStart;
        selectionEnd = maxEnd;

        originalText = textData.Text.Clone();

        subdivisions = GetSubdivisions(selectionStart, selectionEnd, textData.Text, extractEachCharacter);

        subdivisions?.RemoveAll(x => x.text.RawText == "\n");

        if (subdivisions?.Count == 0)
        {
            subdivisions = null;
        }

        if (subdivisions != null)
        {
            newLayerIds = new Guid[subdivisions.Count - 1];

            for (int i = 0; i < newLayerIds.Length; i++)
            {
                newLayerIds[i] = Guid.NewGuid();
            }
        }

        return textData.Text.TextGlyphCount > 0 &&
               minStart >= 0 &&
               maxEnd <= textData.Text.TextGlyphCount &&
               minStart < maxEnd &&
               subdivisions != null;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply,
        out bool ignoreInUndo)
    {
        ignoreInUndo = false;

        var node = target.FindNodeOrThrow<VectorLayerNode>(memberId);

        if (node.EmbeddedShapeData is not TextVectorData textData)
        {
            throw new InvalidOperationException("Node does not contain TextVectorData.");
        }

        List<IChangeInfo> changes = new List<IChangeInfo>();

        for (var index = subdivisions.Count - 1; index >= 0; index--)
        {
            var subdivision = subdivisions[index];

            if (index == 0)
            {
                if (node.EmbeddedShapeData is TextVectorData textVectorData)
                {
                    textVectorData.Text = subdivision.text;
                    textVectorData.TransformationMatrix =
                        textData.TransformationMatrix.PostConcat(
                            Matrix3X3.CreateTranslation(subdivision.offset.X, subdivision.offset.Y));
                }

                var aabb = textData.TransformedVisualAABB.RoundOutwards();
                var affected = new AffectedArea(OperationHelper.FindChunksTouchingRectangle(
                    (RectI)aabb,
                    ChunkyImage.FullChunkSize));

                changes.Add(new VectorShape_ChangeInfo(node.Id, affected));
                continue;
            }

            if (node.EmbeddedShapeData.Clone() is not TextVectorData data)
            {
                throw new InvalidOperationException("Failed to clone TextVectorData.");
            }

            VectorLayerNode newNode = node.Clone() as VectorLayerNode;

            if (newNode == null)
            {
                throw new InvalidOperationException("Failed to clone VectorLayerNode.");
            }

            RichText text = subdivision.text.Clone();

            if (text.RawText.EndsWith("\n"))
            {
                TextInline? lastInline = text.Inlines.LastOrDefault();

                if (lastInline != null)
                {
                    lastInline.Text = lastInline.Text[..^1];

                    if (lastInline.Text.Length == 0)
                    {
                        text.RemoveInline(lastInline);
                    }
                }
            }

            newNode.Id = newLayerIds[index - 1];

            newNode.DisplayName = text.RawText.Length > 20
                ? text.RawText[..20].ReplaceLineEndings("") + "..."
                : text.RawText.ReplaceLineEndings("");

            data.Text = text;
            data.TransformationMatrix =
                textData.TransformationMatrix.PostConcat(Matrix3X3.CreateTranslation(subdivision.offset.X,
                    subdivision.offset.Y));
            newNode.EmbeddedShapeData = data;

            target.NodeGraph.AddNode(newNode);

            changes.Add(CreateLayer_ChangeInfo.FromLayer(newNode));
            changes.AddRange(NodeOperations.AppendMember(node, newNode, out var positions));

            foreach (var position in positions)
            {
                originalPositions[position.Key] = position.Value;
            }
        }

        return changes;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        var node = target.FindNodeOrThrow<VectorLayerNode>(memberId);

        if (node.EmbeddedShapeData is not TextVectorData textData)
        {
            throw new InvalidOperationException("Node does not contain TextVectorData.");
        }

        textData.Text = originalText ?? default;

        List<IChangeInfo> changes = new List<IChangeInfo>();

        AffectedArea affected = new AffectedArea(
            OperationHelper.FindChunksTouchingRectangle(
                (RectI)textData.TransformedVisualAABB.RoundOutwards(),
                ChunkyImage.FullChunkSize));

        changes.Add(new VectorShape_ChangeInfo(node.Id, affected));

        changes.AddRange(NodeOperations.RevertPositions(
            originalPositions,
            target));

        foreach (var newLayerId in newLayerIds)
        {
            var newNode = target.FindNode<VectorLayerNode>(newLayerId);

            if (newNode != null)
            {
                changes.AddRange(NodeOperations.DetachStructureNode(newNode));
                changes.Add(new DeleteStructureMember_ChangeInfo(newLayerId));

                target.NodeGraph.RemoveNode(newNode);
            }
        }

        originalPositions.Clear();

        return changes;
    }

    private List<(RichText text, VecD offset)>? GetSubdivisions(int start, int end,
        RichText text, bool extractEachCharacter)
    {
        if (start == 0 && end == text.TextGlyphCount && !extractEachCharacter)
        {
            return null;
        }

        if (end - start == 1 && start < text.TextGlyphCount && text.GetCharAtCursor(start) == "\n")
        {
            return null;
        }

        var result = new List<(RichText text, VecD offset)>();

        text.IndexOnLine(start, out int startLineIndex);
        text.IndexOnLine(end, out int endLineIndex);

        var glyphPositions = text.GetGlyphPositions(true);

        int globalElementIndex = 0;
        int until = start > 0 ? start : end;
        RichText currentRichText = new RichText();

        var position = VecD.Zero;
        double x = position.X;
        double y = position.Y;
        double boundingWidth = text.MeasureBounds().Width;
        double lastSplitWidth = 0;
        int currentStart = 0;

        for (var index = 0; index < text.Lines.Length; index++)
        {
            var line = text.Lines[index];

            double maxLineHeight = 0;
            double maxFontSize = 0;
            double measuredLineWidth = 0;

            bool allEmpty = true;
            foreach (TextInline inline in line)
            {
                bool isEmpty = string.IsNullOrEmpty(inline.Text) || inline.Text == "\n";
                allEmpty &= isEmpty;
                if (isEmpty) continue;
                if (index > 0)
                {
                    maxLineHeight = Math.Max(maxLineHeight, inline.LineHeight * RichText.PtToPx);
                    maxFontSize = Math.Max(maxFontSize, inline.Font.Size * RichText.PtToPx);
                }

                using Font font = inline.Font.ToFont();
                measuredLineWidth += font.MeasureText(inline.Text);
            }

            if (allEmpty)
            {
                maxLineHeight = line.FirstOrDefault()?.LineHeight * RichText.PtToPx ?? 0;
            }

            double lineX = x;

            TextAlign? alignment = null;

            lastSplitWidth = 0;
            foreach (TextInline inline in line)
            {
                if (string.IsNullOrEmpty(inline.Text) || inline.Text == "\n")
                {
                    globalElementIndex++;
                    continue;
                }

                using Font font = inline.Font.ToFont();
                double alignmentOffset = 0;
                if (alignment == null)
                {
                    alignment = inline.Alignment;

                    if (alignment == TextAlign.Center)
                    {
                        alignmentOffset = (boundingWidth - measuredLineWidth) / 2f;
                    }
                    else if (alignment == TextAlign.Right)
                    {
                        alignmentOffset = (boundingWidth - measuredLineWidth);
                    }
                }

                double topOffset = maxLineHeight;
                double measuredInlineWidth = font.MeasureText(inline.Text);

                VecD inlinePosition = new VecD(lineX + alignmentOffset, y + topOffset);
                double charWidth = 0;

                var textElements = GetTextElements(inline.Text);
                for (int localI = 0; localI <= textElements.Length; localI++)
                {
                    if (globalElementIndex >= until)
                    {
                        if (!string.IsNullOrEmpty(currentRichText.Inlines.FirstOrDefault()?.Text))
                        {
                            var glyphPosition = glyphPositions[currentStart];
                            result.Add(new(currentRichText, (VecD)glyphPosition));
                        }

                        currentRichText = new RichText();
                        currentStart = globalElementIndex;
                        until = FindNextStop(globalElementIndex);
                        lastSplitWidth = charWidth;
                    }

                    if (globalElementIndex < until && localI < textElements.Length)
                    {
                        if (currentRichText.Inlines.Count == 0)
                        {
                            var newInline = inline.Clone();
                            newInline.Text = "";
                            currentRichText.AddInline(newInline);
                        }

                        currentRichText.Inlines.FirstOrDefault().Text += textElements[localI];
                        charWidth += font.MeasureText(textElements[localI]);
                    }

                    if (localI < textElements.Length)
                    {
                        globalElementIndex++;
                    }
                }

                if (globalElementIndex <= until)
                {
                    if (!string.IsNullOrEmpty(currentRichText.Inlines.FirstOrDefault()?.Text))
                    {
                        var glyphPosition = glyphPositions[currentStart];
                        result.Add(new(currentRichText, (VecD)glyphPosition));
                    }

                    currentRichText = new RichText();
                    currentStart = globalElementIndex;
                    until = FindNextStop(globalElementIndex);
                    lastSplitWidth = charWidth;
                }

                lineX += measuredInlineWidth;
            }

            y += maxLineHeight;

            if (globalElementIndex <= until)
            {
                VecD pos = new VecD(x + lastSplitWidth, y);
                if (!string.IsNullOrEmpty(currentRichText.Inlines.FirstOrDefault()?.Text))
                {
                    var glyphPosition = glyphPositions[currentStart];
                    result.Add(new(currentRichText, (VecD)glyphPosition));
                }

                currentRichText = new RichText();
                currentStart = globalElementIndex + 1;
                until = FindNextStop(globalElementIndex);
            }
        }

        return result;
    }

    private int FindNextStop(int globalElementIndex)
    {
        if (globalElementIndex < selectionStart) return selectionStart;
        if (globalElementIndex < selectionEnd) return selectionEnd;

        return originalText.TextGlyphCount;
    }


    private static string[] GetTextElements(string text)
    {
        var elements = new List<string>();

        var enumerator =
            StringInfo.GetTextElementEnumerator(text);

        while (enumerator.MoveNext())
        {
            elements.Add(enumerator.GetTextElement());
        }

        return elements.ToArray();
    }
}
