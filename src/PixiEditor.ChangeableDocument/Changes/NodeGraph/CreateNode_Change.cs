using System.Collections.Immutable;
using System.Reflection;
using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;
using PixiEditor.Numerics;
using Type = System.Type;

namespace PixiEditor.ChangeableDocument.Changes.NodeGraph;

internal class CreateNode_Change : Change
{
    private Type nodeType;
    private Guid id;
    private static Dictionary<Type, INodeFactory> allFactories;
    
    [GenerateMakeChangeAction]
    public CreateNode_Change(Type nodeType, Guid id)
    {
        this.id = id;
        this.nodeType = nodeType;

        if (allFactories == null)
        {
            allFactories = new Dictionary<Type, INodeFactory>();
            var factoryTypes = Assembly.GetExecutingAssembly().GetTypes().Where(x => x.IsSubclassOf(typeof(INodeFactory)) && !x.IsAbstract && !x.IsInterface).ToImmutableArray();
            foreach (var factoryType in factoryTypes)
            {
                INodeFactory factory = (INodeFactory)Activator.CreateInstance(factoryType);
                allFactories.Add(factory.NodeType, factory);
            }
        }
    }
    
    public override bool InitializeAndValidate(Document target)
    {
        return nodeType.IsSubclassOf(typeof(Node));
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        if(id == Guid.Empty)
            id = Guid.NewGuid();

        Node node = null;
        if (allFactories.TryGetValue(nodeType, out INodeFactory factory))
        {
            node = factory.CreateNode(target);
        }
        else
        {
            node = (Node)Activator.CreateInstance(nodeType);
        }
        
        node.Position = new VecD(0, 0);
        node.Id = id;
        target.NodeGraph.AddNode(node);
        ignoreInUndo = false;
        
        return CreateNode_ChangeInfo.CreateFromNode(node); 
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        Node node = target.FindNodeOrThrow<Node>(id);
        target.NodeGraph.RemoveNode(node);
        
        return new DeleteNode_ChangeInfo(id);
    }
}
