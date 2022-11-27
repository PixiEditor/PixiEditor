using PixiEditor.ChangeableDocument.ChangeInfos.Root;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.DrawingApi.Core.Numerics;

namespace PixiEditor.ChangeableDocument.Changes.Root;

internal class ResizeCanvas_Change : ResizeBasedChangeBase
{
    private VecI newSize;
    private readonly ResizeAnchor anchor;

    [GenerateMakeChangeAction]
    public ResizeCanvas_Change(VecI size, ResizeAnchor anchor)
    {
        newSize = size;
        this.anchor = anchor;
    }

    public override bool InitializeAndValidate(Document target)
    {
        if (newSize.X < 1 || newSize.Y < 1)
            return false;
        
        return base.InitializeAndValidate(target);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        if (_originalSize == newSize)
        {
            ignoreInUndo = true;
            return new None();
        }

        target.Size = newSize;
        target.VerticalSymmetryAxisX = Math.Clamp(_originalVerAxisX, 0, target.Size.X);
        target.HorizontalSymmetryAxisY = Math.Clamp(_originalHorAxisY, 0, target.Size.Y);

        VecI offset = anchor.FindOffsetFor(_originalSize, newSize);

        target.ForEveryMember((member) =>
        {
            if (member is Layer layer)
            {
                Resize(layer.LayerImage, layer.GuidValue, newSize, offset, deletedChunks);
            }
            if (member.Mask is null)
                return;

            Resize(member.Mask, member.GuidValue, newSize, offset, deletedMaskChunks);
        });
        
        ignoreInUndo = false;
        return new Size_ChangeInfo(newSize, target.VerticalSymmetryAxisX, target.HorizontalSymmetryAxisY);
    }
}
