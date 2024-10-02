using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.ChangeInfos.Root;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Numerics;

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
        RectI? bounds = null;
        target.ForEveryMember((member) =>
        {
            if (member is LayerNode layer)
            {
                var layerBounds = layer.GetTightBounds(frameToClip);
                if (layerBounds.HasValue)
                {
                    bounds ??= (RectI)layerBounds.Value;
                    bounds = bounds.Value.Union((RectI)layerBounds.Value);
                }
            }
        });

        if (!bounds.HasValue || bounds.Value.IsZeroOrNegativeArea || bounds.Value == new RectI(VecI.Zero, target.Size))
        {
            ignoreInUndo = true;
            return new None();
        }
        
        RectI newBounds = bounds.Value;
        
        target.Size = newBounds.Size;
        target.VerticalSymmetryAxisX = Math.Clamp(_originalVerAxisX, 0, target.Size.X);
        target.HorizontalSymmetryAxisY = Math.Clamp(_originalHorAxisY, 0, target.Size.Y);
        
        target.ForEveryMember((member) =>
        {
            if (member is ImageLayerNode layer)
            {
                layer.ForEveryFrame(img =>
                {
                    Resize(img, layer.Id, newBounds.Size, -newBounds.Pos, deletedChunks);
                });
            }
            
            if (member.EmbeddedMask is null)
                return;
            
            Resize(member.EmbeddedMask, member.Id, newBounds.Size, -newBounds.Pos, deletedMaskChunks);
        });

        ignoreInUndo = false;
        return new Size_ChangeInfo(newBounds.Size, target.VerticalSymmetryAxisX, target.HorizontalSymmetryAxisY);
    }
}
