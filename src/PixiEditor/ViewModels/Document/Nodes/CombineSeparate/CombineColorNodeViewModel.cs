using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.CombineSeparate;
using PixiEditor.ViewModels.Nodes;

namespace PixiEditor.ViewModels.Document.Nodes.CombineSeparate;

[NodeViewModel("COMBINE_COLOR_NODE", "COLOR", PixiPerfectIcons.ItemSlot)]
internal class CombineColorNodeViewModel() : CombineSeparateColorNodeViewModel<CombineColorNode>(true);
