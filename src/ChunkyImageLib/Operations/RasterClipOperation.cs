namespace ChunkyImageLib.Operations
{
    internal class RasterClipOperation : IOperation
    {
        public ChunkyImage ClippingMask { get; }
        public RasterClipOperation(ChunkyImage clippingMask)
        {
            ClippingMask = clippingMask;
        }

        public void Dispose() { }
    }
}
