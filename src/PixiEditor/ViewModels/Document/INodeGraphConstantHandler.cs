namespace PixiEditor.ViewModels.Document;

public interface INodeGraphConstantHandler
{
    public Guid Id { get; }
    
    public string NameBindable { get; }
    
    public object ValueBindable { get; set; }
    
    public Type Type { get; }
}
