using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Shaders;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.FilterNodes;

// This node appears to be too problematic for now, so it is not exposed to the frontend
// Basically, rendering it with high dpi is an enormous pain in the ass, viewport changes affect the looks.
// Filter system would need to be reworked to be executed in "layer space", meaning it is independent of changes to the viewport and stuff
[NodeInfo("NoiseDisplacementFilter")]
public class NoiseDisplacementFilterNode : FilterNode
{
    public const string OffsetProperty = "Offset";
    public InputProperty<double> Scale { get; }
    public InputProperty<bool> Turbulence { get; }
    public InputProperty<VecD> Frequency { get; }
    public InputProperty<int> Octaves { get; }
    public InputProperty<int> Seed { get; }
    //public InputProperty<VecD> Offset { get; }

    private ImageFilter? lastFilter;

    private double lastScale;
    private bool lastTurbulence;
    private VecD lastFrequency;
    private int lastOctaves;
    private double lastSeed;
    private Matrix3X3 lastSurfaceMatrix;
    private VecD lastOffset;

    protected override bool ExecuteOnlyOnCacheChange => false;
    public override bool SupportsIterativeRendering => false;

    public NoiseDisplacementFilterNode()
    {
        Scale = CreateInput("Scale", "SCALE", 20d)
            .WithRules(rules => rules.Min(0d));

        Turbulence = CreateInput("Turbulence", "TURBULENCE_NOISE_PROPERTY", false);

        Frequency = CreateInput("Frequency", "FREQUENCY_PROPERTY", new VecD(0.05, 0.05))
            .WithRules(rules => rules.Min(new VecD(0.0001, 0.0001)));

        Octaves = CreateInput("Octaves", "OCTAVES", 3)
            .WithRules(rules => rules.Min(1).Max(8));

        Seed = CreateInput("Seed", "SEED", 0);

        //Offset = CreateInput(OffsetProperty, "OFFSET", VecD.Zero);
    }

    protected override ImageFilter? GetImageFilter(RenderContext context)
    {
        if (Math.Abs(lastScale - Scale.Value) < float.Epsilon &&
            lastTurbulence == Turbulence.Value &&
            lastFrequency.AlmostEquals(Frequency.Value) &&
            lastOctaves == Octaves.Value &&
            Math.Abs(lastSeed - Seed.Value) < float.Epsilon &&
            lastSurfaceMatrix == context.RenderSurface?.TotalMatrix
            /*&& lastOffset == Offset.Value*/)
            return lastFilter;

        lastFilter?.Dispose();

        lastScale = Scale.Value;
        lastTurbulence = Turbulence.Value;
        lastFrequency = Frequency.Value;
        lastOctaves = Octaves.Value;
        lastSeed = Seed.Value;
        //lastOffset = Offset.Value;

        float adjustedFrequencyX = Math.Max((float)Frequency.Value.X, 0.0001f) / 100f *
                                   (float)context.ChunkResolution.InvertedMultiplier();
        float adjustedFrequencyY = Math.Max((float)Frequency.Value.Y, 0.0001f) / 100f *
                                   (float)context.ChunkResolution.InvertedMultiplier();

        var shader = Turbulence.Value switch
        {
            true => Shader.CreatePerlinNoiseTurbulence(adjustedFrequencyX, adjustedFrequencyY, Octaves.Value, Seed.Value),
            false => Shader.CreatePerlinFractalNoise(adjustedFrequencyX, adjustedFrequencyY, Octaves.Value, Seed.Value),
        };

        float sizeIndependentScale = Scale.Value == 0 ? 0 : (context.RenderSurface?.TotalMatrix.ScaleX ?? 1) / (10f / (float)Scale.Value);
        float adjustedScale = sizeIndependentScale * (float)context.ChunkResolution.Multiplier();

        //VecD normalizedOffset = (Offset.Value / 100f).Multiply(context.RenderSurface.LocalClipBounds.Size);
        //Matrix3X3 matrix = Matrix3X3.CreateTranslation(normalizedOffset.X, normalizedOffset.Y);

        if (context.RenderSurface != null)
        {
            var offseted =
                shader.SetLocalMatrix(
                    context.RenderSurface.TotalMatrix.PostConcat(
                        Matrix3X3.CreateTranslation(
                            adjustedScale / 2f + context.RenderSurface.LocalClipBounds.Size.X,
                            adjustedScale / 2f + context.RenderSurface.LocalClipBounds.Size.Y)
                        /*.PostConcat(matrix)*/));
            shader?.Dispose();
            shader = offseted;
        }
        /*
        else
        {
            var withMatrix = shader.SetLocalMatrix(matrix);
            shader?.Dispose();
            shader = withMatrix;
        }
        */

        lastSurfaceMatrix = context.RenderSurface?.TotalMatrix ?? Matrix3X3.Identity;

        var displacement = ImageFilter.CreateShader(shader, false);
        shader.Dispose();

        float adjustmentOffset = -adjustedScale / (Turbulence.Value ? 2f : 4f);
        var displacementMap =
            ImageFilter.CreateDisplacementMap(ColorChannel.R, ColorChannel.G, adjustedScale, displacement);
        using var offsetMap = ImageFilter.CreateOffset(adjustmentOffset, adjustmentOffset);
        lastFilter = ImageFilter.CreateCompose(offsetMap, displacementMap);
        displacementMap.Dispose();

        displacement.Dispose();

        return lastFilter;
    }

    public override Node CreateCopy() => new NoiseDisplacementFilterNode();

    public override void Dispose()
    {
        base.Dispose();
        lastFilter?.Dispose();
    }
}
