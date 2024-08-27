using System.Collections.ObjectModel;

namespace PixiEditor.ViewModels.Nodes;

public class NodeTypeGroup : PixiObservableObject
{
    public string Name { get; set; }
    public ObservableCollection<NodeTypeInfo> NodeTypes { get; set; }

    public NodeTypeGroup(string name, IEnumerable<NodeTypeInfo> nodeTypes)
    {
        Name = name;
        NodeTypes = new ObservableCollection<NodeTypeInfo>(nodeTypes);
    }    
}
