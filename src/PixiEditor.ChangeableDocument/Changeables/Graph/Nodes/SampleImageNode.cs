using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Shaders.Generation.Expressions;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("SampleImage", "SAMPLE_IMAGE", Category = "IMAGE")]
public class SampleImageNode : Node
{
    public InputProperty<Texture?> Image { get; }

    public FuncInputProperty<Float2> Coordinate { get; }

    public FuncOutputProperty<Half4> Color { get; }

    public SampleImageNode()
    {
        Image = CreateInput<Texture>(nameof(Texture), "IMAGE", null);
        Coordinate = CreateFuncInput<Float2>(nameof(Coordinate), "UV", VecD.Zero);
        Color = CreateFuncOutput(nameof(Color), "COLOR", GetColor);
    }

    private Half4 GetColor(FuncContext context)
    {
        context.ThrowOnMissingContext();

        if (Image.Value is null)
        {
            return new Half4("");
        }

        Float2 uv = context.GetValue(Coordinate);

        return context.SampleTexture(Image.Value, uv);
    }

    protected override Texture? OnExecute(RenderingContext context)
    {
        return Image.Value;
    }

    public override Node CreateCopy() => new SampleImageNode();
}
