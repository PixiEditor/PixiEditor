using ChunkyImageLib.DataHolders;

namespace ChunkyImageLib.Operations
{
    internal record class ResizeOperation : IOperation
    {
        public Vector2i Size { get; }
        public ResizeOperation(Vector2i size)
        {
            Size = size;
        }
        public void Dispose() { }
    }
}
