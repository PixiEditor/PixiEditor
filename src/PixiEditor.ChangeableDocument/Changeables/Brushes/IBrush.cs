using PixiEditor.ChangeableDocument.Changeables.Interfaces;

namespace PixiEditor.ChangeableDocument.Changeables.Brushes;

public interface IBrush
{
    public string? FilePath { get; }
    public Guid Id { get; }
    IReadOnlyDocument Document { get; }
}
