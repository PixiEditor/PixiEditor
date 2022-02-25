using ChangeableDocument.Changeables.Interfaces;

namespace ChangeableDocument.Changeables
{
    internal class Layer : StructureMember, IReadOnlyLayer
    {
        internal override Layer Clone()
        {
            return new Layer()
            {
                GuidValue = GuidValue,
                IsVisible = IsVisible,
                Name = Name
            };
        }
    }
}
