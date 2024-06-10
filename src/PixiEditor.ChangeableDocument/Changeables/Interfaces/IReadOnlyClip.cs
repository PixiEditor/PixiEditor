namespace PixiEditor.ChangeableDocument.Changeables.Interfaces;

public interface IReadOnlyClip
{
    public int StartFrame { get; }
    public int Duration { get; }

    public void ActiveFrameChanged(int atFrame);
    public void Deactivated(int atFrame);
}
