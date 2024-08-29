using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.ViewModels.Nodes;

namespace PixiEditor.ViewModels.Document.Nodes;

internal class SampleImageNodeViewModel : NodeViewModel<SampleImageNode>
{
    public override LocalizedString DisplayName => "SAMPLE_IMAGE";
    
    public override LocalizedString Category => "IMAGE";
}
