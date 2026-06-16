using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

namespace PixiEditor.ChangeableDocument.Helpers;

public static class AffectedAreasUtility
{
    public static AffectedArea GetTightLayerArea(StructureNode node, int frame)
    {
        var tightBounds = node.GetTightBounds(frame);
        HashSet<VecI> chunksToCombine = new();
        if (tightBounds.HasValue)
        {
            VecI chunk = (VecI)tightBounds.Value.TopLeft / ChunkyImage.FullChunkSize;
            VecI sizeInChunks = ((VecI)tightBounds.Value.Size / ChunkyImage.FullChunkSize);
            sizeInChunks = new VecI(Math.Max(1, sizeInChunks.X), Math.Max(1, sizeInChunks.Y));
            for (int x = 0; x < sizeInChunks.X; x++)
            {
                for (int y = 0; y < sizeInChunks.Y; y++)
                {
                    chunksToCombine.Add(chunk + new VecI(x, y));
                }
            }
        }

        return new AffectedArea(chunksToCombine);
    }
}
