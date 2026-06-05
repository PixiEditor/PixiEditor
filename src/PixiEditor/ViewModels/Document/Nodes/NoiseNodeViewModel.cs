using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.UI.Common.Fonts;
using PixiEditor.ViewModels.Nodes;
using PixiEditor.ViewModels.Nodes.Properties;

namespace PixiEditor.ViewModels.Document.Nodes;

[NodeViewModel("NOISE_NODE", "IMAGE", PixiPerfectIcons.Noise)]
internal class NoiseNodeViewModel : NodeViewModel<NoiseNode>
{
    private GenericEnumPropertyViewModel Type { get; set; }
    private GenericEnumPropertyViewModel VoronoiFeature { get; set; }
    private NodePropertyViewModel Randomness { get; set; }
    private NodePropertyViewModel AngleOffset { get; set; }
    private NodePropertyViewModel Lacunarity { get; set; }
    private NodePropertyViewModel Persistence { get; set; }
    private NodePropertyViewModel Dimensions { get; set; }
    private NodePropertyViewModel Z { get; set; }

    public override void OnInitialized()
    {
        Type = FindInputProperty("NoiseType") as GenericEnumPropertyViewModel;
        VoronoiFeature = FindInputProperty("VoronoiFeature") as GenericEnumPropertyViewModel;
        Randomness = FindInputProperty("Randomness");
        AngleOffset = FindInputProperty("AngleOffset");

        Lacunarity = FindInputProperty("Lacunarity");
        Persistence = FindInputProperty("Persistence");
        Dimensions = FindInputProperty("Dimensions");
        Z = FindInputProperty("Z");

        Type.ValueChanged += (_, _) => UpdateInputVisibility();
        Dimensions.ValueChanged += (_, _) => UpdateInputVisibility();
        UpdateInputVisibility();
    }

    private void UpdateInputVisibility()
    {
        if (Type.Value is not NoiseType type)
            return;

        Randomness.IsVisible = type == NoiseType.Voronoi;
        VoronoiFeature.IsVisible = type == NoiseType.Voronoi;
        AngleOffset.IsVisible = type == NoiseType.Voronoi;
        Lacunarity.IsVisible = type is NoiseType.FractalValue or NoiseType.Voronoi or NoiseType.FractalPerlin2
            or NoiseType.FractalVoronoi or NoiseType.FractalSimplexValue or NoiseType.FractalSimplexGradient;
        Persistence.IsVisible = type is NoiseType.FractalValue or NoiseType.Voronoi or NoiseType.FractalPerlin2
            or NoiseType.FractalVoronoi or NoiseType.FractalSimplexValue or NoiseType.FractalSimplexGradient;
        Dimensions.IsVisible = type is NoiseType.FractalValue or NoiseType.FractalPerlin2
            or NoiseType.FractalVoronoi or NoiseType.FractalSimplexValue or NoiseType.FractalSimplexGradient;
        if (Dimensions.Value is not int d)
            return;
        Z.IsVisible = type is NoiseType.FractalValue or NoiseType.FractalPerlin2
            or NoiseType.FractalVoronoi or NoiseType.FractalSimplexValue or NoiseType.FractalSimplexGradient
                      && d == 3;
    }
}
