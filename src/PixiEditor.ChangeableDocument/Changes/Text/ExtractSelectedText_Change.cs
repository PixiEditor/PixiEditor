using ChunkyImageLib.Operations;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Text;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;
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
    private string? originalText = null;
    private List<(int start, int end, string text)> subdividions;
    private Dictionary<Guid, VecD> originalPositions = new Dictionary<Guid, VecD>();


    [GenerateMakeChangeAction]
    public ExtractSelectedText_Change(Guid memberId, int selectionStart, int selectionEnd, bool extractEachCharacter)
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

        originalText = textData.Text;

        subdividions = GetSubdivisions(selectionStart, selectionEnd, textData.Text, extractEachCharacter);

        subdividions?.RemoveAll(x => x.text == "\n");

        if (subdividions?.Count == 0)
        {
            subdividions = null;
        }

        if (subdividions != null)
        {
            newLayerIds = new Guid[subdividions.Count - 1];
            for (int i = 0; i < subdividions.Count - 1; i++)
            {
                newLayerIds[i] = Guid.NewGuid();
            }
        }

        return textData.Text.Length > 0 &&
               minStart >= 0 && maxEnd <= textData.Text.Length &&
               minStart < maxEnd && subdividions != null;
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

        for (var index = subdividions.Count - 1; index >= 0; index--)
        {
            var subdividion = subdividions[index];

            if (index == 0)
            {
                textData.Text = subdividion.text.EndsWith("\n")
                    ? subdividion.text[..^1]
                    : subdividion.text;

                var aabb = textData.TransformedVisualAABB.RoundOutwards();
                var affected = new AffectedArea(OperationHelper.FindChunksTouchingRectangle(
                    (RectI)aabb, ChunkyImage.FullChunkSize));
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

            string text = subdividion.text.EndsWith("\n")
                ? subdividion.text[..^1]
                : subdividion.text;

            newNode.Id = newLayerIds[index - 1];
            newNode.DisplayName = text.Length > 20
                ? text[..20].ReplaceLineEndings("") + "..."
                : text.ReplaceLineEndings("");

            data.Text = text;
            newNode.EmbeddedShapeData = data;
            var newPos = GetPositionForNewText(originalText, subdividion.start, textData);
            data.Position += newPos;

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

        textData.Text = originalText;

        List<IChangeInfo> changes = new List<IChangeInfo>();

        AffectedArea affected = new AffectedArea(OperationHelper.FindChunksTouchingRectangle(
            (RectI)textData.TransformedVisualAABB.RoundOutwards(), ChunkyImage.FullChunkSize));

        changes.Add(new VectorShape_ChangeInfo(node.Id, affected));

        changes.AddRange(NodeOperations.RevertPositions(originalPositions, target));
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

    private VecD GetPositionForNewText(string text, int startIndex, TextVectorData textData)
    {
        RichText richText = new RichText(text);

        var positions = richText.GetGlyphPositions(textData.Font);
        if (positions == null || positions.Length == 0)
        {
            return VecD.Zero;
        }

        VecF position = positions[startIndex];

        richText.IndexOnLine(startIndex, out int lineIndex);

        VecD lineOffset = richText.GetLineOffset(lineIndex, textData.Font);

        return new VecD(position.X, (1 / RichText.PtToPx) * lineOffset.Y);
    }

    private List<(int start, int end, string text)>? GetSubdivisions(int start, int end, string text, bool extractEachCharacter)
    {
        if (start == 0 && end == text.Length && !extractEachCharacter)
            return null;

        if (end - start == 1 && start < text.Length && text[start] == '\n')
            return null;

        var result = new List<(int start, int end, string text)>();

        var richText = new RichText(text);

        richText.IndexOnLine(start, out int startLineIndex);
        richText.IndexOnLine(end, out int endLineIndex);
        bool spansMultipleLines = startLineIndex != endLineIndex;

        int cursor = start;

        if (start > 0)
        {
            result.Add((0, start, text.Substring(0, start)));
            var (startLineStart, startLineEnd) = richText.GetLineStartEnd(startLineIndex);
            bool isMiddleOfLine = start > startLineStart && start < startLineEnd;
            if (isMiddleOfLine && spansMultipleLines)
            {
                int substringLength = Math.Min(startLineEnd - start, text.Length - start);
                result.Add((start, startLineEnd, text.Substring(start, substringLength)));
                cursor = startLineEnd;
            }
        }

        if (cursor < end)
        {
            if (extractEachCharacter)
            {
                for(int i = cursor; i < end; i++)
                {
                    result.Add((i, i + 1, text.Substring(i, 1)));
                }
            }
            else
            {
                result.Add((cursor, end, text.Substring(cursor, end - cursor)));
            }
            cursor = end;

            if (cursor >= text.Length)
                return result;

            var (endLineStart, endLineEnd) = richText.GetLineStartEnd(endLineIndex);
            bool endsMiddleOfLine = end > endLineStart && end < endLineEnd;
            if (endsMiddleOfLine)
            {
                int substringLength = Math.Min(endLineEnd - end, text.Length - end);
                result.Add((end, endLineEnd, text.Substring(end, substringLength)));
                cursor = endLineEnd;
            }
        }

        if (cursor < text.Length)
        {
            result.Add((cursor, text.Length, text.Substring(cursor)));
        }

        return result;
    }


    /*private List<(int start, int end, string text)>? GetSubdivisions(int start, int end, string text)
    {
        if (start == 0 && end == text.Length)
        {
            return null;
        }

        RichText richText = new RichText(text);
        richText.IndexOnLine(start, out int startLineIndex);
        richText.IndexOnLine(end, out int endLineIndex);

        bool isExtractingFromMiddle = start > 0 || end < text.Length;

        if (startLineIndex == endLineIndex && !isExtractingFromMiddle)
        {
            return [(start, end, text.Substring(start, end - start))];
        }

        // returns lineStart and lineEnd char indices for the given line index
        var firstLineLength = richText.GetLineStartEnd(startLineIndex);


    }*/
}
