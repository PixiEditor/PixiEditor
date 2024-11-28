namespace PixiEditor.Models.Handlers;

internal interface ISearchHandler : IHandler
{
    public void OpenSearchWindow(string searchQuery, bool searchAll = true);
}
