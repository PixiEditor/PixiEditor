using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.ViewModels.Nodes;

namespace PixiEditor.ViewModels.Document.Nodes;

internal class CreateImageNodeViewModel : NodeViewModel<CreateImageNode>
{
    public override LocalizedString DisplayName => "CREATE_IMAGE_NODE";
    
    public override LocalizedString Category => "IMAGE";
}
