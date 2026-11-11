using PixiEditor.ChangeableDocument.Changeables.Interfaces;

namespace PixiEditor.ChangeableDocument.Changeables;

public class Guide : IReadOnlyGuide
{
    public Guid Id { get; }
    public bool IsVertical { get; }
    public double Offset { get; set; }

    public Guide(Guid id, bool isVertical, double axisOffset)
    {
        Id = id;
        IsVertical = isVertical;
        Offset = axisOffset;
    }
}
