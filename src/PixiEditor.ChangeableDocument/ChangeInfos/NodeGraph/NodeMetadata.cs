using System.Reflection;
using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;

namespace PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;

public class NodeMetadata
{
    public bool IsPairNode { get; private set; }
    public bool IsPairNodeStart { get; private set; }
    public bool IsPairNodeEnd => IsPairNode && !IsPairNodeStart;

    public Guid? PairNodeGuid { get; set; }
    public string? ZoneUniqueName { get; private set; }
    
    public Type NodeType { get; private set; }

    public NodeMetadata(Type type)
    {
        NodeType = type;
        AddAttributes(type);
    }
    
    public NodeMetadata(IReadOnlyNode node) : this(node.GetType()) { }

    private void AddAttributes(Type type)
    {
        AddPairAttributes(type);
    }

    private void AddPairAttributes(Type type)
    {
        var attribute = type.GetCustomAttribute<PairNodeAttribute>();

        if (attribute == null)
            return;

        ZoneUniqueName = attribute.ZoneUniqueName;
        IsPairNode = true;
        IsPairNodeStart = attribute.IsStartingType;
    }
}
