using Drawie.Backend.Core.Vector;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors.Features;

public interface IPathExecutor
{
    public void OnPathChanged(VectorPath path);
}
