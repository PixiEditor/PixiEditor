using Drawie.Backend.Core.Numerics;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.ChangeableDocument.Changeables;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.Models.Handlers;
using PixiEditor.ViewModels.Nodes;
using PixiEditor.ViewModels.Tools.Tools;

namespace PixiEditor.ViewModels.Document.Nodes;

[NodeViewModel("NESTED_DOCUMENT", null, null)]
internal class NestedDocumentNodeViewModel :
    StructureMemberViewModel<ChangeableDocument.Changeables.Graph.Nodes.NestedDocumentNode>, ILayerHandler
{
    bool lockTransparency;
    Guid? referenceId = null;
    string? _linkedDocumentPath = null;

    public Matrix3X3 TransformationMatrix => (Internals.Tracker.Document.FindMember(Id) as ITransformableObject)
        ?.TransformationMatrix ?? Matrix3X3.Identity;

    public void SetLockTransparency(bool lockTransparency)
    {
        this.lockTransparency = lockTransparency;
        OnPropertyChanged(nameof(LockTransparencyBindable));
    }

    public bool LockTransparencyBindable
    {
        get => lockTransparency;
        set
        {
            if (!Document.BlockingUpdateableChangeActive)
                Internals.ActionAccumulator.AddFinishedActions(new LayerLockTransparency_Action(Id, value));
        }
    }

    private bool shouldDrawOnMask = false;

    public bool ShouldDrawOnMask
    {
        get => shouldDrawOnMask;
        set
        {
            if (value == shouldDrawOnMask)
                return;
            shouldDrawOnMask = value;
            OnPropertyChanged(nameof(ShouldDrawOnMask));
        }
    }

    public Type? QuickEditTool => typeof(MoveToolViewModel);

    public bool IsLinked => _linkedDocumentPath != null || (referenceId != null && referenceId != Guid.Empty);
    public string? LinkedDocumentPath => _linkedDocumentPath ?? referenceId?.ToString();

    public void SetOriginalFilePath(string? infoOriginalFilePath)
    {
        _linkedDocumentPath = infoOriginalFilePath;
        OnPropertyChanged(nameof(IsLinked));
        OnPropertyChanged(nameof(LinkedDocumentPath));
    }

    public void SetReferenceId(Guid? infoReferenceId)
    {
        referenceId = infoReferenceId;
        OnPropertyChanged(nameof(IsLinked));
        OnPropertyChanged(nameof(LinkedDocumentPath));
    }
}
