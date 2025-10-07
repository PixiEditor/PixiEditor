using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.ChangeInfos.Root;
using Drawie.Backend.Core.Numerics;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changes.Root;

internal class Crop_Change : ResizeBasedChangeBase
{
    private RectI rect;
    
    [GenerateMakeChangeAction]
    public Crop_Change(RectI rect)
    {
        this.rect = rect;
    }


    public override bool InitializeAndValidate(Document target)
    {
        return base.InitializeAndValidate(target) && rect is { Width: > 0, Height: > 0 };
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        if (_originalSize == rect.Size)
        {
            ignoreInUndo = true;
            return new None();
        }

        target.Size = rect.Size;
        target.VerticalSymmetryAxisX = Math.Clamp(_originalVerAxisX - rect.Pos.X, 0, rect.Size.X);
        target.HorizontalSymmetryAxisY = Math.Clamp(_originalHorAxisY - rect.Pos.Y, 0, rect.Size.Y);

        target.ForEveryMember((member) =>
        {
            if (member is ImageLayerNode layer)
            {
                layer.ForEveryFrame((frame, id) =>
                {
                    Resize(frame, id, rect.Size, rect.Pos * -1, deletedChunks);
                });
            }
            if (member.EmbeddedMask is null)
                return;

            Resize(member.EmbeddedMask, member.Id, rect.Size, rect.Pos * -1, deletedMaskChunks);
        });
        
        ignoreInUndo = false;
        return new Size_ChangeInfo(rect.Size, target.VerticalSymmetryAxisX, target.HorizontalSymmetryAxisY);
    }
}
