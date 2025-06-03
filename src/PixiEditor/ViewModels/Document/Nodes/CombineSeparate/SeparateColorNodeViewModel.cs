using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.CombineSeparate;
using PixiEditor.UI.Common.Fonts;
using PixiEditor.ViewModels.Nodes;

namespace PixiEditor.ViewModels.Document.Nodes.CombineSeparate;

[NodeViewModel("SEPARATE_COLOR_NODE", "COLOR", PixiPerfectIcons.ItemSlotOut)]
internal class SeparateColorNodeViewModel() : CombineSeparateColorNodeViewModel<SeparateColorNode>(false);
