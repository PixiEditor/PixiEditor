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
    private int selectionStart;
    private int selectionEnd;
    private string? originalText = null;
    private List<(int start, int end, string text)> subdividions;
    private Dictionary<Guid, VecD> originalPositions = new Dictionary<Guid, VecD>();


    [GenerateMakeChangeAction]
    public ExtractSelectedText_Change(Guid memberId, int selectionStart, int selectionEnd)
    {
        this.memberId = memberId;
        this.selectionStart = selectionStart;
        this.selectionEnd = selectionEnd;
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

        subdividions = GetSubdivisions(selectionStart, selectionEnd, textData.Text);

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
                textData.Text = subdividion.text.ReplaceLineEndings("");
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

            string text = subdividion.text.ReplaceLineEndings("");
            newNode.Id = newLayerIds[index - 1];
            newNode.DisplayName = text.Length > 20
                ? text[..20] + "..."
                : text;

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
        return new None();
        /*var node = target.FindNodeOrThrow<VectorLayerNode>(memberId);
        if (node.EmbeddedShapeData is not TextVectorData textData)
        {
            throw new InvalidOperationException("Node does not contain TextVectorData.");
        }

        textData.Text = originalText;

        List<IChangeInfo> changes = new List<IChangeInfo>();

        if (nestedActions != null)
        {
            foreach (var action in nestedActions)
            {
                changes.AddRange(action.Revert(target).AsT2);
            }
        }

        AffectedArea affected = new AffectedArea(OperationHelper.FindChunksTouchingRectangle(
            (RectI)textData.TransformedVisualAABB.RoundOutwards(), ChunkyImage.FullChunkSize));

        changes.Add(new VectorShape_ChangeInfo(node.Id, affected));

        var newNode = target.FindNode<VectorLayerNode>(newLayerId);
        if (newNode != null)
        {
            changes.AddRange(NodeOperations.DetachStructureNode(newNode));
            changes.AddRange(NodeOperations.RevertPositions(originalPositions, target));
            changes.Add(new DeleteNode_ChangeInfo(newLayerId));

            target.NodeGraph.RemoveNode(newNode);
        }

        originalPositions.Clear();

        return changes;*/
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

    private List<(int start, int end, string text)>? GetSubdivisions(int start, int end, string text)
    {
        if (start == 0 && end == text.Length)
            return null;

        var result = new List<(int start, int end, string text)>();
        var richText = new RichText(text);

        if (start > 0)
            result.Add((0, start, text.Substring(0, start)));

        int cursor = start;
        int adjustedEnd = end;

        while (cursor < adjustedEnd)
        {
            richText.IndexOnLine(cursor, out int lineIndex);
            var (lineStart, lineEnd) = richText.GetLineStartEnd(lineIndex); // lineEnd is exclusive

            int segmentStart = cursor;
            int segmentEnd = Math.Min(adjustedEnd, lineEnd);

            // If selection ends exactly before line break, include the \n
            if (segmentEnd < lineEnd - 1 && text[segmentEnd] == '\n' && segmentEnd + 1 == lineEnd)
            {
                segmentEnd += 1;
                adjustedEnd += 1; // shift selection forward so suffix doesn't get the newline
            }

            result.Add((segmentStart, segmentEnd, text.Substring(segmentStart, segmentEnd - segmentStart)));
            cursor = segmentEnd;
        }

        if (adjustedEnd < text.Length)
            result.Add((adjustedEnd, text.Length, text.Substring(adjustedEnd)));


        result.RemoveAll(x => x.text == "\n");
        if (result.Count == 0)
            return null;

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
