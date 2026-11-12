using Drawie.Numerics;
using PixiEditor.ChangeableDocument.ChangeInfos.Root;

namespace PixiEditor.ChangeableDocument.Changes.Root;

internal class ResetLastBrushToolUseEndPosition_Change : Change
{
    [GenerateMakeChangeAction]
    public ResetLastBrushToolUseEndPosition_Change() { }
    
    public override bool InitializeAndValidate(Document target)
    {
        return true;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        target.LastBrushToolUseEndPosition = null;
        ignoreInUndo = true;
        return new LastBrushToolUsePosition_ChangeInfo(null);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        return new None();
    }
}
