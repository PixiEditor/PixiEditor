using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.CombineSeparate;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.ViewModels.Nodes;

namespace PixiEditor.ViewModels.Document.Nodes.CombineSeparate;

internal class SeparateChannelsNodeViewModel : NodeViewModel<SeparateChannelsNode>
{
    public override LocalizedString DisplayName => "SEPARATE_CHANNELS_NODE";
    
    public override LocalizedString Category => "IMAGE";
}
