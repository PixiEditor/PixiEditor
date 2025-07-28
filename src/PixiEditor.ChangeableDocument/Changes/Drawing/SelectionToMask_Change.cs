using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.Changes.Drawing.FloodFill;
using PixiEditor.ChangeableDocument.Enums;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;
using BlendMode = Drawie.Backend.Core.Surfaces.BlendMode;

namespace PixiEditor.ChangeableDocument.Changes.Drawing;

internal class SelectionToMask_Change : Change
{
    private readonly SelectionMode mode;
    private readonly Guid targetMember;
    private Changeables.Selection? selection;
    private CommittedChunkStorage? chunkStorage = null;
    private int frame;
    
    [GenerateMakeChangeAction]
    public SelectionToMask_Change(Guid targetMember, SelectionMode mode, int frame)
    {
        this.targetMember = targetMember;
        this.mode = mode;
        this.frame = frame;
    }
    
    public override bool InitializeAndValidate(Document target)
    {
        selection = target.Selection;
        return true;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        var image = DrawingChangeHelper.GetTargetImageOrThrow(target, targetMember, true, frame);
        
        VectorPath? selection = target.Selection.SelectionPath.IsEmpty ? null : target.Selection.SelectionPath;
        HashSet<Guid> membersToReference = new();
        membersToReference.Add(targetMember);
        
        var blendMode = mode switch
        {
            SelectionMode.New => BlendMode.DstATop,
            SelectionMode.Add => BlendMode.Plus,
            SelectionMode.Subtract => BlendMode.DstOut,
            SelectionMode.Intersect => BlendMode.SrcIn
        };

        image.SetBlendMode(blendMode);

        var selectionImage = FloodFillHelper.FillSelection(target, selection!);

        image.EnqueueDrawImage(new VecI(0, 0), selectionImage);
        
        var affArea = image.FindAffectedArea();
        chunkStorage = new CommittedChunkStorage(image, affArea.Chunks);
        image.CommitChanges();

        ignoreInUndo = false;
        return DrawingChangeHelper.CreateAreaChangeInfo(targetMember, affArea, true);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        var affArea = DrawingChangeHelper.ApplyStoredChunksDisposeAndSetToNull(target, targetMember, true, frame, ref chunkStorage);
        return DrawingChangeHelper.CreateAreaChangeInfo(targetMember, affArea, true);
    }
}
