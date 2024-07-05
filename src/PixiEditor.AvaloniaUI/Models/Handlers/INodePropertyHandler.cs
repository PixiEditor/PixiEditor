namespace PixiEditor.AvaloniaUI.Models.Handlers;

public interface INodePropertyHandler
{
    public string Name { get; set; }
    public object Value { get; set; }
    public bool IsInput { get; }
    public INodeHandler Node { get; set; }
}
