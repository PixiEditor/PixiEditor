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

    public InputProperty<bool> NormalizedCoordinates { get; }

    public SampleImageNode()
    {
        Image = CreateInput<Texture>("Texture", "IMAGE", null);
        Coordinate = CreateFuncInput<Float2>("Coordinate", "UV", VecD.Zero);
        Color = CreateFuncOutput("Color", "COLOR", GetColor);
        SampleMode = CreateInput("SampleMode", "COLOR_SAMPLE_MODE", ColorSampleMode.ColorManaged);
        NormalizedCoordinates = CreateInput("NormalizedCoordinates", "NORMALIZE_COORDINATES", true);
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

            return context.SampleSurface(Image.Value.DrawingSurface, uv, SampleMode.Value, NormalizedCoordinates.Value);
        }

        Color color;

        VecD coordinate = context.GetValue(Coordinate).ConstantValue;
        VecI pixelCoordinate = (VecI)coordinate.Round();

        if(NormalizedCoordinates.Value)
        {
            VecD size = Image.Value.Size;
            pixelCoordinate = (VecI)(new VecD(coordinate.X * size.X, coordinate.Y * size.Y)).Round();
        }

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
