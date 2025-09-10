using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Utility;

[NodeInfo("RepeatEnd")]
[PairNode(typeof(RepeatNodeStart), "RepeatZone", false)]
public class RepeatNodeEnd : Node
{
    protected override void OnExecute(RenderContext context)
    {
        throw new NotImplementedException();
    }

    public override Node CreateCopy()
    {
        throw new NotImplementedException();
    }
}
