namespace PixiEditor.AvaloniaUI.Models.Handlers;

public interface IInputPropertyHandler : INodePropertyHandler
{
    bool INodePropertyHandler.IsInput => true;
    public IOutputPropertyHandler? Connection { get; set; }
}
