using ChunkyImageLib.DataHolders;
using Drawie.Backend.Core.Numerics;
using Drawie.Numerics;
using PixiEditor.Models.Handlers;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors.Features;

public interface ITransformableExecutor : IExecutorFeature
{
    public bool IsTransforming { get; }
    public void OnTransformChanged(ShapeCorners corners); 
    public void OnTransformApplied();
    public void OnLineOverlayMoved(VecD start, VecD end);
    public void OnSelectedObjectNudged(VecI distance);
    public bool IsTransformingMember(Guid id);
}
