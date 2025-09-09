using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.ChangeInfos.Root;
using PixiEditor.ChangeableDocument.Enums;
using Drawie.Backend.Core.Numerics;
using Drawie.Numerics;

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

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply,
        out bool ignoreInUndo)
    {
        if (_originalSize == newSize)
        {
            ignoreInUndo = true;
            return new None();
        }

        target.Size = newSize;
        float normalizedX = (float)_originalVerAxisX / _originalSize.X;
        float normalizedY = (float)_originalHorAxisY / _originalSize.Y;
        float newVerticalSymmetryAxisX = newSize.X * normalizedX;
        float newHorizontalSymmetryAxisY = newSize.Y * normalizedY;
        target.VerticalSymmetryAxisX = Math.Clamp(newVerticalSymmetryAxisX, 0, target.Size.X);
        target.HorizontalSymmetryAxisY = Math.Clamp(newHorizontalSymmetryAxisY, 0, target.Size.Y);

        VecI offset = anchor.FindOffsetFor(_originalSize, newSize);

        target.ForEveryMember((member) =>
        {
            if (member is ImageLayerNode layer)
            {
                layer.ForEveryFrame((img, id) =>
                {
                    Resize(img, id, newSize, offset, deletedChunks);
                });
            }

            // TODO: Check if adding support for different Layer types is necessary

            if (member.EmbeddedMask is null)
                return;

            Resize(member.EmbeddedMask, member.Id, newSize, offset, deletedMaskChunks);
        });

        ignoreInUndo = false;
        return new Size_ChangeInfo(newSize, target.VerticalSymmetryAxisX, target.HorizontalSymmetryAxisY);
    }
}
