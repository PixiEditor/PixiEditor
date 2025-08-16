using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.ChangeInfos.Root;
using Drawie.Backend.Core.Numerics;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;

namespace PixiEditor.ChangeableDocument.Changes.Root;

internal class ClipCanvas_Change : ResizeBasedChangeBase
{
    private int frameToClip;
    [GenerateMakeChangeAction]
    public ClipCanvas_Change(int clipToFrame)
    {
        frameToClip = clipToFrame;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        RectD? bounds = null;
        target.ForEveryMember((member) =>
        {
            if (member.IsVisible.Value)
            {
                if (member is LayerNode layer)
                {
                    var layerBounds = layer.GetTightBounds(frameToClip);
                    if (layerBounds is { IsZeroOrNegativeArea: false })
                    {
                        bounds ??= layerBounds.Value;
                        bounds = bounds.Value.Union(layerBounds.Value);
                    }
                }
            }
        });

        if (!bounds.HasValue || bounds.Value.IsZeroOrNegativeArea || bounds.Value == new RectD(VecI.Zero, target.Size))
        {
            ignoreInUndo = true;
            return new None();
        }
        
        RectD newBounds = bounds.Value;
        
        VecI size = (VecI)newBounds.Size.Ceiling();
        
        target.Size = size;
        target.VerticalSymmetryAxisX = Math.Clamp(_originalVerAxisX, 0, Math.Max(target.Size.X, 1));
        target.HorizontalSymmetryAxisY = Math.Clamp(_originalHorAxisY, 0, Math.Max(target.Size.Y, 1));
        
        target.ForEveryMember((member) =>
        {
            if (member is ImageLayerNode layer)
            {
                layer.ForEveryFrame(img =>
                {
                    Resize(img, layer.Id, size, -(VecI)newBounds.Pos, deletedChunks);
                });
            }
            else if (member is ITransformableObject transformableObject)
            {
                originalTransformations[member.Id] = transformableObject.TransformationMatrix;
                VecD floor = new VecD(-(float)newBounds.Pos.X, -(float)newBounds.Pos.Y);
                transformableObject.TransformationMatrix = transformableObject.TransformationMatrix.PostConcat(Matrix3X3.CreateTranslation((float)floor.X, (float)floor.Y));
            }

            if (member.EmbeddedMask is null)
                return;
            
            Resize(member.EmbeddedMask, member.Id, size, -(VecI)newBounds.Pos, deletedMaskChunks);
        });

        ignoreInUndo = false;
        return new Size_ChangeInfo(size, target.VerticalSymmetryAxisX, target.HorizontalSymmetryAxisY);
    }
}
