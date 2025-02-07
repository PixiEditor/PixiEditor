using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.CombineSeparate;
using PixiEditor.ViewModels.Nodes;

namespace PixiEditor.ViewModels.Document.Nodes.CombineSeparate;

[NodeViewModel("SEPARATE_COLOR_NODE", "COLOR", "\uE813")]
internal class SeparateColorNodeViewModel() : CombineSeparateColorNodeViewModel<SeparateColorNode>(false);
