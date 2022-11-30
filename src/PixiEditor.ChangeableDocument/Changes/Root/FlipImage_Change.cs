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
        var changes = Flip(target);
        
        ignoreInUndo = false;
        return changes;
    }

    private void FlipImage(ChunkyImage img)
    {
        using Paint paint = new()
        {
            BlendMode = DrawingApi.Core.Surface.BlendMode.Src
        };
        
        using Surface originalSurface = new(img.LatestSize);
        img.DrawMostUpToDateRegionOn(
            new(VecI.Zero, img.LatestSize), 
            ChunkResolution.Full,
            originalSurface.DrawingSurface,
            VecI.Zero);

        using Surface flipped = new Surface(img.LatestSize);

        bool flipX = flipType == FlipType.Horizontal;
        bool flipY = flipType == FlipType.Vertical;
        
        flipped.DrawingSurface.Canvas.Save();
                flipped.DrawingSurface.Canvas.Scale(flipX ? -1 : 1, flipY ? -1 : 1, flipX ? img.LatestSize.X / 2f : 0,
            flipY ? img.LatestSize.Y / 2f : 0f);
        flipped.DrawingSurface.Canvas.DrawSurface(originalSurface.DrawingSurface, 0, 0, paint);
        flipped.DrawingSurface.Canvas.Restore();
        
        img.EnqueueClear();
        img.EnqueueDrawImage(VecI.Zero, flipped);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        return Flip(target);
    }

    private OneOf<None, IChangeInfo, List<IChangeInfo>> Flip(Document target)
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

        return changes;
    }
}
