namespace PixiEditor.Models.Containers;

internal interface ISearchHandler : IHandler
{
    public void OpenSearchWindow(string searchQuery);
}
