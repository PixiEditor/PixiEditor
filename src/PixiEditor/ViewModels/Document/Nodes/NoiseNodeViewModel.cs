using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.ViewModels.Nodes;

namespace PixiEditor.ViewModels.Document.Nodes;

internal class NoiseNodeViewModel : NodeViewModel<NoiseNode>
{
    public override LocalizedString DisplayName => "NOISE_NODE";
    
    public override LocalizedString Category => "IMAGE";
}
