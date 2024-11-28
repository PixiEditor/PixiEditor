using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Vector;

namespace PixiEditor.ChangeableDocument.Changeables;

internal class Selection : IReadOnlySelection, IDisposable
{
    public static Color SelectionColor { get; } = Colors.CornflowerBlue;
    public VectorPath SelectionPath { get; set; } = new();
    VectorPath IReadOnlySelection.SelectionPath 
    {
        get 
        {
            try
            {
                // I think this might throw if another thread disposes SelectionPath at the wrong time?
                return new VectorPath(SelectionPath) { FillType = PathFillType.EvenOdd };
            }
            catch (Exception)
            {
                return new VectorPath();
            }
        }
    }

    public void Dispose()
    {
        SelectionPath.Dispose();
    }
}
