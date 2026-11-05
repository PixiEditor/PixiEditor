using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.ChangeableDocument.Changeables;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changes.Structure;
using PixiEditor.Tests;

namespace PixiEditor.ChangeableDocument.Tests;

public class RasterizeMemberTests : PixiEditorTest
{
    [Fact]
    public void RasterizePreservesBlendModeAndOpacity()
    {
        using Document document = new() { Size = new(16, 16) };
        VectorLayerNode layer = new();
        layer.BlendMode.NonOverridenValue = BlendMode.Multiply;
        layer.Opacity.NonOverridenValue = 0.5f;
        document.NodeGraph.AddNode(layer);

        Guid newId = Guid.NewGuid();
        using RasterizeMember_Change change = new(layer.Id, newId, 0);

        Assert.True(change.InitializeAndValidate(document));
        change.Apply(document, true, out _);

        var rasterized = (ImageLayerNode)document.FindMember(newId);
        Assert.Equal(BlendMode.Multiply, rasterized.BlendMode.NonOverridenValue);
        Assert.Equal(0.5f, rasterized.Opacity.NonOverridenValue);
    }
}
