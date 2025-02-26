using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.FilterNodes;

[NodeInfo("GrayscaleFilter")]
public class GrayscaleNode : FilterNode
{
    private static readonly ColorMatrix WeightedMatrix = ColorMatrix.WeightedWavelengthGrayscale + ColorMatrix.UseAlpha;
    private static readonly ColorMatrix AverageMatrix = ColorMatrix.AverageGrayscale + ColorMatrix.UseAlpha;
    
    public InputProperty<GrayscaleMode> Mode { get; }
    
    public InputProperty<double> Factor { get; }
    
    public InputProperty<bool> Normalize { get; }

    public InputProperty<Vec3D> CustomWeight { get; }
    
    private GrayscaleMode lastMode;
    private double lastFactor;
    private bool lastNormalize;
    private Vec3D lastCustomWeight;

    private ColorFilter? filter;
    
    public GrayscaleNode()
    {
        Mode = CreateInput("Mode", "MODE", GrayscaleMode.Weighted);
        Factor = CreateInput("Factor", "FACTOR", 1d)
            .WithRules(rules => rules.Min(0d).Max(1d));
        Normalize = CreateInput("Normalize", "NORMALIZE", true);
        CustomWeight = CreateInput("CustomWeight", "WEIGHT_FACTOR", new Vec3D(1, 1, 1));
    }

    protected override ColorFilter? GetColorFilter()
    {
        if (Mode.Value == lastMode 
            && Factor.Value == lastFactor 
            && Normalize.Value == lastNormalize &&
            CustomWeight.Value == lastCustomWeight)
        {
            return filter;
        }
        
        lastMode = Mode.Value;
        lastFactor = Factor.Value;
        lastNormalize = Normalize.Value;
        lastCustomWeight = CustomWeight.Value;

        filter?.Dispose();
        
        var matrix = Mode.Value switch
        {
            GrayscaleMode.Weighted => UseFactor(WeightedMatrix),
            GrayscaleMode.Average => UseFactor(AverageMatrix),
            GrayscaleMode.Custom => UseFactor(ColorMatrix.WeightedGrayscale(GetAdjustedCustomWeight()) +
                                              ColorMatrix.UseAlpha)
        };

        filter = ColorFilter.CreateColorMatrix(matrix);
        return filter;
    }

    private ColorMatrix UseFactor(ColorMatrix target)
    {
        var factor = Factor.Value;

        return factor switch
        {
            0 => ColorMatrix.Identity,
            1 => target,
            _ => ColorMatrix.Lerp(ColorMatrix.Identity, target, (float)factor)
        };
    }

    private Vec3D GetAdjustedCustomWeight()
    {
        var weight = CustomWeight.Value;
        var normalize = Normalize.Value;

        if (!normalize)
        {
            return weight;
        }

        var sum = weight.Sum();

        if (sum == 0)
        {
            return Vec3D.Zero;
        }
            
        return weight / weight.Sum();
    }

    public override Node CreateCopy() => new GrayscaleNode();

    public enum GrayscaleMode
    {
        Weighted,
        Average,
        Custom
    }
}
