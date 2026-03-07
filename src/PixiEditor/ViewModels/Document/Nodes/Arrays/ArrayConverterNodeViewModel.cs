using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Arrays;
using PixiEditor.ViewModels.Nodes;

namespace PixiEditor.ViewModels.Document.Nodes.Arrays;

[NodeViewModel("ARRAY_CONVERTER", null, null)]
internal class ArrayConverterNodeViewModel : NodeViewModel<ArrayConverterNode>
{
    public override bool IsSocketConverterNode => true;
}
