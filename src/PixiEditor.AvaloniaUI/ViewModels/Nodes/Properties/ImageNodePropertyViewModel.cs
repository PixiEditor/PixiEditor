using ChunkyImageLib;

namespace PixiEditor.AvaloniaUI.ViewModels.Nodes.Properties;

public class ImageNodePropertyViewModel : NodePropertyViewModel<Surface>
{
    public ImageNodePropertyViewModel(NodeViewModel node) : base(node)
    {
        this.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(Value))
            {
                Node.ResultPreview = Value;
            }
        };
    }
}
