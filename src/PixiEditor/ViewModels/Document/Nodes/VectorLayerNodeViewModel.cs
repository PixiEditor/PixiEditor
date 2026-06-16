using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces.Shapes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.Models.Handlers;
using PixiEditor.UI.Common.Fonts;
using PixiEditor.ViewModels.Nodes;
using PixiEditor.ViewModels.Tools.Tools;

namespace PixiEditor.ViewModels.Document.Nodes;

[NodeViewModel("VECTOR_LAYER", "STRUCTURE", PixiPerfectIcons.VectorPen)]
internal class VectorLayerNodeViewModel : StructureMemberViewModel<VectorLayerNode>, IVectorLayerHandler, ITransformableMemberHandler
{
    private Dictionary<Type, Type> quickToolsMap = new Dictionary<Type, Type>()
    {
        { typeof(IReadOnlyEllipseData), typeof(VectorEllipseToolViewModel) },
        { typeof(IReadOnlyRectangleData), typeof(VectorRectangleToolViewModel) },
        { typeof(IReadOnlyLineData), typeof(VectorLineToolViewModel) },
        { typeof(IReadOnlyTextData), typeof(TextToolViewModel) },
        { typeof(IReadOnlyPathData), typeof(VectorPathToolViewModel) }
    };
    
    bool lockTransparency;
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

    public Type? QuickEditTool
    {
        get
        {
            var shapeData = GetShapeData(Document.AnimationDataViewModel.ActiveFrameTime);
            if (shapeData is null)
                return null;

            foreach (var tool in quickToolsMap)
            {
                if(shapeData.GetType().IsAssignableTo(tool.Key))
                    return tool.Value;
            }
            
            return null;
        }
    }

    public IReadOnlyShapeVectorData? GetShapeData(KeyFrameTime frameTime)
    {
        return ((IReadOnlyVectorNode)Internals.Tracker.Document.FindMember(Id))?.ShapeData;
    }
}
