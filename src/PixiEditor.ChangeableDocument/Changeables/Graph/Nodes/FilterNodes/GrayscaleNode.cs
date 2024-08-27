using PixiEditor.DrawingApi.Core.Surfaces.PaintImpl;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.FilterNodes;

[NodeInfo("GrayscaleFilter", "GRAYSCALE_FILTER_NODE", Category = "FILTERS")]
public class GrayscaleNode : FilterNode
{
    private static readonly ColorMatrix WeightedMatrix = ColorMatrix.WeightedWavelengthGrayscale + ColorMatrix.UseAlpha;
    private static readonly ColorMatrix AverageMatrix = ColorMatrix.AverageGrayscale + ColorMatrix.UseAlpha;
    
    public InputProperty<GrayscaleMode> Mode { get; }
    
    public InputProperty<double> Factor { get; }
    
    public InputProperty<bool> Normalize { get; }

    // TODO: Hide when Mode != Custom
    public InputProperty<VecD3> CustomWeight { get; }
    
    private GrayscaleMode lastMode;
    private double lastFactor;
    private bool lastNormalize;
    private VecD3 lastCustomWeight;
    
    private ColorFilter? filter;
    
    public GrayscaleNode()
    {
        Mode = CreateInput("Mode", "MODE", GrayscaleMode.Weighted);
        // TODO: Clamp 0 - 1 in UI
        Factor = CreateInput("Factor", "FACTOR", 1d);
        Normalize = CreateInput("Normalize", "NORMALIZE", true);
        CustomWeight = CreateInput("CustomWeight", "WEIGHT_FACTOR", new VecD3(1, 1, 1));
    }

    protected override ColorFilter GetColorFilter()
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
        
        filter = ColorFilter.CreateColorMatrix(Mode.Value switch
        {
            GrayscaleMode.Weighted => UseFactor(WeightedMatrix),
            GrayscaleMode.Average => UseFactor(AverageMatrix),
            GrayscaleMode.Custom => UseFactor(ColorMatrix.WeightedGrayscale(GetAdjustedCustomWeight()) +
                                              ColorMatrix.UseAlpha)
        });
        
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

    private VecD3 GetAdjustedCustomWeight()
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
            return VecD3.Zero;
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
