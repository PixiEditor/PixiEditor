using Drawie.Backend.Core.Vector;

namespace PixiEditor.ChangeableDocument.Changeables.Interfaces;

public interface IReadOnlySelection
{
    /// <summary>
    /// The path of the selection
    /// </summary>
    public VectorPath SelectionPath { get; }
}
