using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.ViewModels.Nodes;

namespace PixiEditor.ViewModels.Document.Nodes;

internal class ModifyImageRightNodeViewModel : NodeViewModel<ModifyImageRightNode>
{
    public override LocalizedString DisplayName => "MODIFY_IMAGE_RIGHT_NODE";
    
    public override LocalizedString Category => "IMAGE";
}
