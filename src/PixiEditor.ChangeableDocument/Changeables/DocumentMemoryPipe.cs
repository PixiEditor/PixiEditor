using PixiEditor.ChangeableDocument.Changeables.Interfaces;

namespace PixiEditor.ChangeableDocument.Changeables;

internal abstract class DocumentMemoryPipe<T> : ICrossDocumentPipe<T> where T : class
{
    protected Document Document { get; }
    public bool IsOpen { get; private set; }
    public bool CanOpen { get; private set; } = true;

    public DocumentMemoryPipe(Document document)
    {
        Document = document;
        IsOpen = false;
    }

    public void Open()
    {
        if (!CanOpen)
            throw new InvalidOperationException("Pipe cannot be opened");
        IsOpen = true;
    }

    public void Close()
    {
        IsOpen = false;
    }

    public T? TryAccessData()
    {
        if (!IsOpen)
        {
            if (!CanOpen)
            {
#if DEBUG
                throw new InvalidOperationException("Trying to open a disposed pipe");
#endif
                return null;
            }

            Open();
        }

        if (!DocumentValid()) return null;

        return GetData();
    }

    protected abstract T? GetData();

    public void Dispose()
    {
        IsOpen = false;
        CanOpen = false;
    }

    private bool DocumentValid() => Document is { IsDisposed: false };
}
