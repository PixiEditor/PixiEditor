using PixiEditor.DrawingApi.Core.Surfaces.PaintImpl;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.FilterNodes;

[NodeInfo("GrayscaleFilter", "GRAYSCALE_FILTER_NODE")]
public class GrayscaleNode : FilterNode
{
    public InputProperty<GrayscaleMode> Mode { get; }
    
    public InputProperty<bool> Normalize { get; }

    // TODO: Hide when Mode != Custom
    public InputProperty<VecD3> CustomWeight { get; }
    
    public GrayscaleNode()
    {
        Mode = CreateInput("Mode", "MODE", GrayscaleMode.Weighted);
        Normalize = CreateInput("Normalize", "NORMALIZE", true);
        CustomWeight = CreateInput("CustomWeight", "WEIGHT", new VecD3(1, 1, 1));
    }

    protected override ColorFilter GetColorFilter() => ColorFilter.CreateColorMatrix(Mode.Value switch
    {
        GrayscaleMode.Weighted => ColorMatrix.WeightedWavelengthGrayscale + ColorMatrix.UseAlpha,
        GrayscaleMode.Average => ColorMatrix.AverageGrayscale + ColorMatrix.UseAlpha,
        GrayscaleMode.Custom => ColorMatrix.WeightedGrayscale(GetAdjustedCustomWeight()) + ColorMatrix.UseAlpha
    });

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
