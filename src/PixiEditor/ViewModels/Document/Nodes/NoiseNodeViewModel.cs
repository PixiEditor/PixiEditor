using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.UI.Common.Fonts;
using PixiEditor.ViewModels.Nodes;
using PixiEditor.ViewModels.Nodes.Properties;

namespace PixiEditor.ViewModels.Document.Nodes;

[NodeViewModel("NOISE_NODE", "IMAGE", PixiPerfectIcons.Noise)]
internal class NoiseNodeViewModel : NodeViewModel<NoiseNode>
{
    private GenericEnumPropertyViewModel Type { get; set; }
    private NodePropertyViewModel Randomness { get; set; }

    public override void OnInitialized()
    {
        Type = FindInputProperty("NoiseType") as GenericEnumPropertyViewModel;
        Randomness = FindInputProperty("Randomness");

        Type.ValueChanged += (_, _) => UpdateInputVisibility();
        UpdateInputVisibility();
    }

    private void UpdateInputVisibility()
    {
        if (Type.Value is not NoiseType type)
            return;
        
        Randomness.IsVisible = type == NoiseType.Voronoi;
    }
}
