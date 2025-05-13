using Drawie.Backend.Core;
using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changes.NodeGraph;

internal class EvaluateGraph_Change : Change
{
    private readonly Guid endNodeGuid;
    private readonly KeyFrameTime frameTime;

    [GenerateMakeChangeAction]
    public EvaluateGraph_Change(Guid endNodeGuid, KeyFrameTime frameTime)
    {
        this.endNodeGuid = endNodeGuid;
        this.frameTime = frameTime;
    }

    public override bool InitializeAndValidate(Document target)
    {
        return target.HasNode(endNodeGuid);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply,
        out bool ignoreInUndo)
    {
        ignoreInUndo = true;

        var node = target.FindNode(endNodeGuid);
        var queue = GraphUtils.CalculateExecutionQueue(node);

        using Texture renderTexture = Texture.ForProcessing(target.Size, target.ProcessingColorSpace);
        RenderContext context =
            new(renderTexture.DrawingSurface, frameTime, ChunkResolution.Full, target.Size,
                target.ProcessingColorSpace) { FullRerender = true };
        foreach (var nodeToEvaluate in queue)
        {
            nodeToEvaluate.Execute(context);
        }

        return new None();
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        return new None();
    }
}
