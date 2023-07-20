namespace PixiEditor.Models.Containers;

internal interface IToolsHandler : IHandler
{
    public void SetTool(object parameter);
    public void RestorePreviousTool();
}
