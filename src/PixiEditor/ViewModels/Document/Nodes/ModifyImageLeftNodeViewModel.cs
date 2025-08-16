using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.UI.Common.Fonts;
using PixiEditor.ViewModels.Nodes;

namespace PixiEditor.ViewModels.Document.Nodes;

[NodeViewModel("MODIFY_IMAGE_LEFT_NODE", "IMAGE", PixiPerfectIcons.PutImage)]
internal class ModifyImageLeftNodeViewModel : NodeViewModel<ModifyImageLeftNode>, IPairNodeStartViewModel;
