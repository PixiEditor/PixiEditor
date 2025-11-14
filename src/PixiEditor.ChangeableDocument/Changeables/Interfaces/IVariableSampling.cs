using Drawie.Backend.Core.Surfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph;

namespace PixiEditor.ChangeableDocument.Changeables.Interfaces;

public interface IVariableSampling
{
    public InputProperty<bool> BilinearSampling { get; }
}
