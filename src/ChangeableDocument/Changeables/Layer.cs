using ChangeableDocument.Changeables.Interfaces;
using ChunkyImageLib;

namespace ChangeableDocument.Changeables
{
    internal class Layer : StructureMember, IReadOnlyLayer
    {
        public ChunkyImage LayerImage { get; set; } = new();
        IReadOnlyChunkyImage IReadOnlyLayer.ReadOnlyLayerImage => LayerImage;

        internal override Layer Clone()
        {
            return new Layer()
            {
                ReadOnlyGuidValue = ReadOnlyGuidValue,
                ReadOnlyIsVisible = ReadOnlyIsVisible,
                ReadOnlyName = ReadOnlyName,
                ReadOnlyOpacity = ReadOnlyOpacity,
                LayerImage = LayerImage.CloneFromLatest()
            };
        }
    }
}
