using ChunkyImageLib.DataHolders;

namespace ChunkyImageLib.Operations;

internal record class ResizeOperation : IOperation
{
    public VecI Size { get; }
    public ResizeOperation(VecI size)
    {
        Size = size;
    }
    public void Dispose() { }
}
