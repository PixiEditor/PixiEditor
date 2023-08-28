namespace PixiEditor.AvaloniaUI.Models.Handlers;

internal interface IToolsHandler : IHandler
{
    public void SetTool(object parameter);
    public void RestorePreviousTool();
}
