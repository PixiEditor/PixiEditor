using Drawie.Backend.Core.Vector;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors.Features;

public interface IPathExecutor : IExecutorFeature
{
    public void OnPathChanged(VectorPath path);
}
