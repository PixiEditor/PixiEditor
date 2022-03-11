using ChangeableDocument.Changeables.Interfaces;
using ChunkyImageLib;

namespace ChangeableDocument.Changeables
{
    internal class Layer : StructureMember, IReadOnlyLayer
    {
        public ChunkyImage LayerImage { get; set; } = new();
        IReadOnlyChunkyImage IReadOnlyLayer.LayerImage => LayerImage;

        internal override Layer Clone()
        {
            return new Layer()
            {
                GuidValue = GuidValue,
                IsVisible = IsVisible,
                Name = Name,
                Opacity = Opacity
            };
        }
    }
}
