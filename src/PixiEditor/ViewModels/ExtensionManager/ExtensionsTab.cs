namespace PixiEditor.ViewModels.ExtensionManager;

public class ExtensionsTab
{
    public string Id { get; }
    public string Name { get; }
    
    public ExtensionsTab(string id, string name)
    {
        Id = id;
        Name = name;
    }
}
