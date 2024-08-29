using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.ViewModels.Nodes;

namespace PixiEditor.ViewModels.Document.Nodes;

internal class ModifyImageLeftNodeViewModel : NodeViewModel<ModifyImageLeftNode>
{
    public override LocalizedString DisplayName => "MODIFY_IMAGE_LEFT_NODE";
    
    public override LocalizedString Category => "IMAGE";
}
