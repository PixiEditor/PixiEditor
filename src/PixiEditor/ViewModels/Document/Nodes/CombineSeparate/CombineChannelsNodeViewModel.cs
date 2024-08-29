using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.CombineSeparate;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.ViewModels.Nodes;

namespace PixiEditor.ViewModels.Document.Nodes.CombineSeparate;

internal class CombineChannelsNodeViewModel : NodeViewModel<CombineChannelsNode>
{
    public override LocalizedString DisplayName => "COMBINE_CHANNELS_NODE";
    
    public override LocalizedString Category => "IMAGE";
}
