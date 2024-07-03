using Avalonia.Controls;
using PixiEditor.AvaloniaUI.ViewModels.Nodes;

namespace PixiEditor.AvaloniaUI.Views.Nodes.Properties;

public abstract class NodePropertyView<T> : UserControl
{
    protected void SetValue(T value)
    {
        if (DataContext is NodePropertyViewModel<T> viewModel)
        {
            viewModel.Value = value;
        }
    }
}
