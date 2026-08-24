namespace PixiEditor.Helpers.UI;

internal sealed class CompositeDisposable : IDisposable
{
    private readonly List<IDisposable> items = new();
    
    public int Count => items.Count;

    public void Add(IDisposable disposable)
    {
        items.Add(disposable);
    }

    public void Dispose()
    {
        foreach (var item in items)
            item.Dispose();

        items.Clear();
    }
}

internal sealed class ActionDisposable : IDisposable
{
    private readonly Action action;

    public ActionDisposable(Action action)
    {
        this.action = action;
    }

    public void Dispose()
    {
        action();
    }
}
