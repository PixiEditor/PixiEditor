using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Shaders.Generation;
using Drawie.Backend.Core.Shaders.Generation.Expressions;
using Drawie.Backend.Core.Surfaces;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("SampleImage")]
public class SampleImageNode : Node
{
    public InputProperty<Texture?> Image { get; }

    public FuncInputProperty<Float2> Coordinate { get; }

    public FuncOutputProperty<Half4> Color { get; }

    public InputProperty<ColorSampleMode> SampleMode { get; }

    public SampleImageNode()
    {
        Image = CreateInput<Texture>(nameof(Texture), "IMAGE", null);
        Coordinate = CreateFuncInput<Float2>(nameof(Coordinate), "UV", VecD.Zero);
        Color = CreateFuncOutput(nameof(Color), "COLOR", GetColor);
        SampleMode = CreateInput(nameof(SampleMode), "COLOR_SAMPLE_MODE", ColorSampleMode.ColorManaged);
    }

    private Half4 GetColor(FuncContext context)
    {
        if (Image.Value is null || Image.Value.IsDisposed)
        {
            return new Half4("");
        }

        if (context.HasContext)
        {
            Expression uv = context.GetValue(Coordinate);

            return context.SampleSurface(Image.Value.DrawingSurface, uv, SampleMode.Value);
        }

        Color color;
        VecI pixelCoordinate = (VecI)context.GetValue(Coordinate).ConstantValue.Round();
        if (SampleMode.Value == ColorSampleMode.ColorManaged)
        {
            color = Image.Value.GetSRGBPixel(pixelCoordinate);
        }
        else
        {
            color = Image.Value.GetPixel(pixelCoordinate);
        }

        return new Half4("") { ConstantValue = color };
    }

    protected override void OnExecute(RenderContext context)
    {
    }

    public override Node CreateCopy() => new SampleImageNode();
}
