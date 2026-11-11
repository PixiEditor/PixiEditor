namespace PixiEditor.Models.Handlers;

public interface IGuideHandler
{
    Guid Id { get; }
    bool IsVertical { get; }
    double Offset { get; }
    void SetOffsetInternal(double newOffset);
}
