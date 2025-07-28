namespace PixiEditor.ChangeableDocument.Changeables.Interfaces;

public interface ICrossDocumentPipe<T> : IDisposable
{
    public T? TryAccessData();
    public bool CanOpen { get; }
    public bool IsOpen { get; }
    public void Open();
    public void Close();
}
