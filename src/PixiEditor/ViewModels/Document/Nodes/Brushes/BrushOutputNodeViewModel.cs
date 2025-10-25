using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Brushes;
using PixiEditor.ViewModels.Nodes;

namespace PixiEditor.ViewModels.Document.Nodes.Brushes;

[NodeViewModel("BRUSH_OUTPUT_NODE", "BRUSHES", null)]
internal class BrushOutputNodeViewModel : NodeViewModel<BrushOutputNode>
{
    public override void OnInitialized()
    {
        InputPropertyMap[BrushOutputNode.BrushNameProperty].SocketEnabled = false;
        InputPropertyMap[BrushOutputNode.FitToStrokeSizeProperty].SocketEnabled = false;
    }
}
