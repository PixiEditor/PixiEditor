using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Shaders.Generation.Expressions;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("SampleImage")]
public class SampleImageNode : Node
{
    public InputProperty<Texture?> Image { get; }

    public FuncOutputProperty<Float2> Coordinate { get; }

    public FuncOutputProperty<Half4> Color { get; }

    public override string DisplayName { get; set; } = "SAMPLE_IMAGE";

    public SampleImageNode()
    {
        Image = CreateInput<Texture>(nameof(Texture), "IMAGE", null);
        Coordinate = CreateFuncOutput(nameof(Coordinate), "UV", ctx => ctx.Position);
        Color = CreateFuncOutput(nameof(Color), "COLOR", GetColor);
    }

    private Half4 GetColor(FuncContext context)
    {
        context.ThrowOnMissingContext();

        if (Image.Value is null)
        {
            return new Half4("");
        }

        return context.SampleTexture(Image.Value, context.Position);
    }

    protected override Texture? OnExecute(RenderingContext context)
    {
        return Image.Value;
    }

    public override Node CreateCopy() => new SampleImageNode();
}
