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

    public override void OnInitialized()
    {
        Type = FindInputProperty("NoiseType") as GenericEnumPropertyViewModel;
        VoronoiFeature = FindInputProperty("VoronoiFeature") as GenericEnumPropertyViewModel;
        Randomness = FindInputProperty("Randomness");
        AngleOffset = FindInputProperty("AngleOffset");

        Type.ValueChanged += (_, _) => UpdateInputVisibility();
        UpdateInputVisibility();
    }

    private void UpdateInputVisibility()
    {
        if (Type.Value is not NoiseType type)
            return;
        
        Randomness.IsVisible = type == NoiseType.Voronoi;
        VoronoiFeature.IsVisible = type == NoiseType.Voronoi;
        AngleOffset.IsVisible = type == NoiseType.Voronoi;
    }
}
