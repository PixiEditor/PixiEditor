using System.Collections.Generic;
using System.Linq;
using PixiEditor.AvaloniaUI.Models.Handlers.Tools;
using PixiEditor.AvaloniaUI.Models.Tools;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.ChangeableDocument.Actions.Undo;
using PixiEditor.ChangeableDocument.Enums;

namespace PixiEditor.AvaloniaUI.Models.DocumentModels.UpdateableChangeExecutors;

internal class MagicWandToolExecutor : UpdateableChangeExecutor
{
    private bool considerAllLayers;
    private bool drawOnMask;
    private List<Guid> memberGuids;
    private SelectionMode mode;

    public override ExecutionState Start()
    {
        var magicWand = GetHandler<IMagicWandToolHandler>();
        var members = document!.ExtractSelectedLayers(true);

        if (magicWand is null || members.Count == 0)
            return ExecutionState.Error;

        mode = magicWand.SelectMode;
        memberGuids = members;
        considerAllLayers = magicWand.DocumentScope == DocumentScope.AllLayers;
        if (considerAllLayers)
            memberGuids = document!.StructureHelper.GetAllLayers().Select(x => x.GuidValue).ToList();
        var pos = controller!.LastPixelPosition;

        internals!.ActionAccumulator.AddActions(new MagicWand_Action(memberGuids, pos, mode));

        return ExecutionState.Success;
    }

    public override void OnLeftMouseButtonUp()
    {
        internals!.ActionAccumulator.AddActions(new ChangeBoundary_Action());
        onEnded!(this);
    }

    public override void ForceStop()
    {
        internals!.ActionAccumulator.AddActions(new ChangeBoundary_Action());
    }
}
