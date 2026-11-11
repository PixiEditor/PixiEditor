namespace PixiEditor.ChangeableDocument.Changeables.Interfaces;

public interface IReadOnlyGuide
{
    bool IsVertical { get; }
    double Offset { get; }
}
