using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Drawie.Backend.Core.Numerics;
using Drawie.Numerics;

namespace ChunkyImageLib.DataHolders;

/// <summary>
/// The affected area is defined as the intersection between AffectedArea.Chunks and AffectedArea.GlobalArea. 
/// In other words, a pixel is considered to be affected when both of those are true:
/// 1. The pixel falls inside the GlobalArea rectangle;
/// 2. The Chunks collection contains the chunk that the pixel belongs to.
/// The GlobalArea == null case is treated as "nothing was affected".
/// </summary>
public struct AffectedArea
{
    public HashSet<VecI> Chunks { get; set; }

    /// <summary>
    /// A rectangle in global full-scale coordinates
    /// </summary>
    public RectI? GlobalArea { get; set; }

    public AffectedArea()
    {
        Chunks = new();
        GlobalArea = null;
    }

    public AffectedArea(HashSet<VecI> chunks)
    {
        Chunks = chunks;
        if (chunks.Count == 0)
        {
            GlobalArea = null;
            return;
        }
        GlobalArea = new RectI(chunks.First(), new(ChunkyImage.FullChunkSize));
        foreach (var vec in chunks)
        {
            GlobalArea = GlobalArea.Value.Union(new RectI(vec * ChunkyImage.FullChunkSize, new(ChunkyImage.FullChunkSize)));
        }
    }

    public AffectedArea(HashSet<VecI> chunks, RectI? globalArea)
    {
        GlobalArea = globalArea;
        Chunks = chunks;
    }

    public AffectedArea(AffectedArea original)
    {
        Chunks = new HashSet<VecI>(original.Chunks);
        GlobalArea = original.GlobalArea;
    }

    public void UnionWith(AffectedArea other)
    {
        Chunks.UnionWith(other.Chunks);

        if (GlobalArea is not null && other.GlobalArea is not null)
            GlobalArea = GlobalArea.Value.Union(other.GlobalArea.Value);
        else
            GlobalArea = GlobalArea ?? other.GlobalArea;
    }

    public void ExceptWith(HashSet<VecI> otherChunks) => ExceptWith(new AffectedArea(otherChunks));

    public void ExceptWith(AffectedArea other)
    {
        Chunks.ExceptWith(other.Chunks);
        if (GlobalArea is null || other.GlobalArea is null)
            return;

        RectI overlap = GlobalArea.Value.Intersect(other.GlobalArea.Value);

        if (overlap.IsZeroOrNegativeArea)
            return;

        if (overlap == other.GlobalArea.Value)
        {
            Chunks = new();
            GlobalArea = null;
            return;
        }

        if (overlap.Width == GlobalArea.Value.Width)
        {
            if (overlap.Top == GlobalArea.Value.Top)
                GlobalArea = GlobalArea.Value with { Top = overlap.Bottom };
            else if (overlap.Bottom == GlobalArea.Value.Bottom)
                GlobalArea = GlobalArea.Value with { Bottom = overlap.Top };
            return;
        }

        if (overlap.Height == GlobalArea.Value.Height)
        {
            if (overlap.Left == GlobalArea.Value.Left)
                GlobalArea = GlobalArea.Value with { Left = overlap.Right };
            else if (overlap.Right == GlobalArea.Value.Right)
                GlobalArea = GlobalArea.Value with { Right = overlap.Left };
            return;
        }
    }
}
