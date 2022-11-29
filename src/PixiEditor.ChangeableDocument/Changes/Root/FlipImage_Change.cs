using ChunkyImageLib.Operations;
using PixiEditor.ChangeableDocument.ChangeInfos.Root;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.PaintImpl;
using BlendMode = PixiEditor.ChangeableDocument.Enums.BlendMode;

namespace PixiEditor.ChangeableDocument.Changes.Root;

internal sealed class FlipImage_Change : Change
{
    private readonly FlipType flipType;
    
    [GenerateMakeChangeAction]
    public FlipImage_Change(FlipType flipType)
    {
        this.flipType = flipType;
    }
    
    public override bool InitializeAndValidate(Document target)
    {
        return true;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        List<IChangeInfo> changes = new List<IChangeInfo>();

        target.ForEveryMember(member =>
        {
            if (member is Layer layer)
            {
                FlipImage(layer.LayerImage);
                changes.Add(new LayerImageChunks_ChangeInfo(member.GuidValue, layer.LayerImage.FindAffectedChunks()));
                layer.LayerImage.CommitChanges();
            }

            if (member.Mask is not null)
            {
                FlipImage(member.Mask);
                member.Mask.CommitChanges();
            }
        });
        
        ignoreInUndo = false;
        return changes;
    }

    private void FlipImage(ChunkyImage img)
    {
        using Paint paint = new()
        {
            BlendMode = DrawingApi.Core.Surface.BlendMode.Src
        };
        
        /*using Surface originalSurface = new(img.LatestSize);
        img.DrawMostUpToDateRegionOn(
            new(VecI.Zero, img.LatestSize), 
            ChunkResolution.Full,
            originalSurface.DrawingSurface,
            VecI.Zero);

        using Surface flipped = new Surface(img.LatestSize);
        
        flipped.DrawingSurface.Canvas.Save();
        flipped.DrawingSurface.Canvas.Scale(flipType == FlipType.Horizontal ? -1 : 1, flipType == FlipType.Vertical ? -1 : 1);
        flipped.DrawingSurface.Canvas.DrawSurface(originalSurface.DrawingSurface, 0, 0, paint);
        flipped.DrawingSurface.Canvas.Restore();*/

        ChunkyImage copy = img.CloneFromCommitted();
        using Surface originalSurface = new(img.LatestSize);
        img.DrawMostUpToDateRegionOn(
            new(VecI.Zero, img.LatestSize), 
            ChunkResolution.Full,
            originalSurface.DrawingSurface,
            VecI.Zero);
        
        img.EnqueueClear();
        Matrix3X3 matrix = Matrix3X3.Identity;

        matrix.ScaleX = -1f;
        img.EnqueueDrawImage(matrix, originalSurface, paint);

        //img.EnqueueDrawChunkyImage(VecI.Zero, copy, flipType == FlipType.Horizontal, flipType == FlipType.Vertical);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        return new None();
    }
}
