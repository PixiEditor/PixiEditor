namespace PixiEditor.Models.Handlers;

public interface IGuideHandler
{
    Guid Id { get; }
    bool IsVertical { get; }
    double OffsetBindable { get; }
    void SetOffsetInternal(double newOffset);
}
